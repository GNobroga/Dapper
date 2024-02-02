using System.Data;
using Dapper;
using DapperTesting.Factories;
using DapperTesting.Models;
namespace DapperTesting.Repositories.Impl;

public class UsuarioRepository(IConnectionFactory connectionFactory) : IUsuarioRepository
{
    private readonly IDbConnection _connection = connectionFactory.Connection;

    public async Task<Usuario> CreateAsync(Usuario usuario)
    {
        return await connectionFactory.AtomicOperation(async (transaction) => {

            var sql = """
                INSERT INTO Usuarios 
                    (Nome, Email, Sexo, RG, CPF, NomeMae, SituacaoCadastro, DataCadastro)
                VALUES 
                    (@Nome, @Email, @Sexo, @RG, @CPF, @NomeMae, @SituacaoCadastro, @DataCadastro);
                
                    SELECT last_insert_rowid();
            """;

             int id = await _connection.QueryFirstAsync<int>(sql, usuario, transaction);

            if (usuario.Contato is not null)
            {
                sql = """
                    INSERT INTO Contatos 
                        (UsuarioId, Telefone, Celular)
                    VALUES (@UsuarioId, @Telefone, @Celular);
                """;

                usuario.Contato.UsuarioId = id;
                await _connection.ExecuteAsync(sql, usuario.Contato, transaction);
            }

            if (usuario.EnderecoEntregas is not null && usuario.EnderecoEntregas is { Count: > 0})
            {
                sql = """
                    INSERT INTO EnderecosEntrega 
                        (UsuarioId, NomeEndereco, CEP, Estado, Bairro, Endereco, Numero, Complemento, Cidade)
                    VALUES 
                        (@UsuarioId, @NomeEndereco, @CEP, @Estado, @Bairro, @Endereco, @Numero, @Complemento, @Cidade);
                """;

                foreach (var enderecoEntrega in usuario.EnderecoEntregas)
                {
                    Console.WriteLine(enderecoEntrega.CEP);
                    enderecoEntrega.UsuarioId = id;
                    await _connection.ExecuteAsync(sql, enderecoEntrega);
                }
            }

            return usuario;
        });
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var usuario = await FindByIdAsync(id);

        if (usuario is null) return false;

        var sql = "DELETE FROM Usuarios WHERE Id = @id";
        await _connection.ExecuteAsync(sql, new { id });

        return true;
    }


    public async Task<List<Usuario>> FindAllAsync()
    {

        // A consulta precisa ser feita na ordem pra poder dar o SplitOn no identificador.
        var sql = """
           SELECT * FROM 
                Usuarios u
                LEFT JOIN Contatos c ON C.UsuarioId = u.Id
                LEFT JOIN EnderecosEntrega ee ON ee.UsuarioId = u.Id;
               
        """;

        List<Usuario> usuarios = [];

        // O Dapper vai trazer várias colunas parecidas, pois é uma relação 1 para N entre usuario e EndereçoEntrega.
        await _connection.QueryAsync<Usuario, Contato, EnderecoEntrega, Usuario>(sql, (usuario, contato, enderecoEntrega) => {

            if (!usuarios.Any(x => x.Id == usuario.Id))
            {
                usuario.EnderecoEntregas ??= [];
                usuario.Contato = contato;
                usuarios.Add(usuario);
            } 
            else 
            {
                usuario = usuarios.Find(x => x.Id == usuario.Id)!;
            }

             usuario.EnderecoEntregas.Add(enderecoEntrega);

            return usuario;
        });

        return usuarios;
    }

    public async Task<Usuario?> FindByIdAsync(int id)
    {
        // Relacionamento N x N
        var sql = """
            SELECT u.*, c.*, ee.*, d.* FROM 
            Usuarios u
            LEFT JOIN Contatos c ON C.UsuarioId = u.Id
            LEFT JOIN EnderecosEntrega ee ON ee.UsuarioId = u.Id
            INNER JOIN UsuariosDepartamentos ud ON ud.UsuarioId = u.Id
            INNER JOIN Departamentos d ON d.Id = ud.DepartamentoId
            WHERE u.Id = @id;
        """;

         List<Usuario> usuarios = [];

        await _connection.QueryAsync<Usuario, Contato, EnderecoEntrega, Departamento, Usuario>(sql, (usuario, contato, enderecoEntrega, departamento) => {

            if (!usuarios.Any(x => x.Id == usuario.Id))
            {
                usuario.EnderecoEntregas ??= [];
                usuario.Contato = contato;
                usuarios.Add(usuario);
            } 
            else 
            {
                usuario = usuarios.Find(x => x.Id == usuario.Id)!;
            }

             usuario.EnderecoEntregas.Add(enderecoEntrega);
            
            if (!usuario.Departamentos.Any(x => x.Id == departamento.Id)) 
            {
                 usuario.Departamentos.Add(departamento);
            }
            

            return usuario;
        }, new { id });

        return usuarios.FirstOrDefault();
    }

    public async Task<Usuario> UpdateAsync(Usuario usuario)
    {

        return await connectionFactory.AtomicOperation<Usuario>(async (transaction) => {

             var sql = """
                UPDATE Usuarios 
                    SET Nome = @Nome, Email = @Email, Sexo = @Sexo, RG = @RG, CPF = @CPF, NomeMae = @NomeMae, SituacaoCadastro = @SituacaoCadastro, DataCadastro = @DataCadastro
                WHERE 
                    Usuarios.Id = @Id;
            """;

            if (usuario.Contato is not null)
            {   
                var sqlContact = "UPDATE Contatos SET Telefone = @Telefone, Celular = @Celular WHERE UsuarioId = @UsuarioId";

                usuario.Contato!.UsuarioId = usuario.Id;

                await _connection.ExecuteAsync(sqlContact, usuario.Contato, transaction);
            }

            if (usuario.EnderecoEntregas is not null && usuario.EnderecoEntregas is { Count: > 0})
            {   

                sql = "DELETE FROM EnderecosEntrega WHERE UsuarioId = @id";

                foreach (var enderecoEntrega in usuario.EnderecoEntregas)
                {
                    await _connection.ExecuteAsync(sql, new { id = usuario.Id });
                    sql = """
                        INSERT INTO EnderecosEntrega 
                            (UsuarioId, NomeEndereco, CEP, Estado, Bairro, Endereco, Numero, Complemento, Cidade)
                        VALUES 
                            (@UsuarioId, @NomeEndereco, @CEP, @Estado, @Bairro, @Endereco, @Numero, @Complemento, @Cidade);
                    """;
                    await _connection.ExecuteAsync(sql, enderecoEntrega);
                }
            }

            await _connection.ExecuteAsync(sql, usuario, transaction);

            return (await FindByIdAsync(usuario.Id))!;
        });


    }
}