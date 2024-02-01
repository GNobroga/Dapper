using System.Data;
using Dapper;
using DapperTesting.DatabaseFactories;
using DapperTesting.Models;
namespace DapperTesting.Repositories.Impl;

public class UsuarioRepository(IConnectionFactory connectionFactory) : IUsuarioRepository
{
    private readonly IDbConnection _connection = connectionFactory.CreateConnection();

    public async Task<Usuario> CreateAsync(Usuario usuario)
    {

        _connection.Open();

        var transaction = _connection.BeginTransaction();

        try
        {
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
            transaction.Commit();
            return (await FindByIdAsync(id))!;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
        finally
        {
            _connection.Close();
        }
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
        // var sql = "SELECT * FROM Usuarios;";
        // var usuarios = await _connection.QueryAsync<Usuario>(sql);

        var sql = """
            SELECT * FROM 
                Usuarios u, EnderecosEntrega ee, Contatos c  
            WHERE 
                u.Id = ee.UsuarioId AND 
                u.Id = c.UsuarioId;
        """;

        Dictionary<int, Usuario> consult = [];

        var usuarios = await _connection.QueryAsync<Usuario, Contato, EnderecoEntrega, Dictionary<int, Usuario>>(sql, (usuario, contato, enderecoEntrega) => {
            if (!consult.TryGetValue(usuario.Id, out Usuario? value))
            {
                usuario.Contato = contato;
                consult.Add(usuario.Id, usuario);
            }
            else 
            {
                var usuarioRecuperado = value;
                if (!usuarioRecuperado.EnderecoEntregas.Any(x => x.Id == enderecoEntrega.Id))
                    usuarioRecuperado.EnderecoEntregas.Add(enderecoEntrega);
            }
            return consult;
        });

        return usuarios.SelectMany(x => x.Values).ToList();
    }

    public async Task<Usuario?> FindByIdAsync(int id)
    {
        var sql = "SELECT * FROM Usuarios u LEFT JOIN Contatos c ON u.Id = c.UsuarioId WHERE u.Id = @id";

        var usuarios = await _connection.QueryAsync<Usuario, Contato, Usuario>(sql, (usuario, contato) =>
        {
            usuario.Contato = contato;
            return usuario;
        }, new { id });

        return usuarios.FirstOrDefault();
    }

    public async Task<Usuario> UpdateAsync(Usuario usuario)
    {

        var transaction = _connection.BeginTransaction();
        _connection.Open();

        try
        {

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

            await _connection.ExecuteAsync(sql, usuario, transaction);

            transaction.Commit();
            return (await FindByIdAsync(usuario.Id))!;
        } 
        catch 
        {
            transaction.Rollback();
            throw;
        }
        finally 
        {
            _connection.Close();
        }
        
    }
}