# Testando o Dapper

Neste projeto, desenvolvo uma pequena API com a abordagem minimalista, utilizando o Dapper como ORM para mapear as colunas para um objeto de destino. No contexto dessa implementação, faço uso de diferentes formas de relacionamento, como One-to-One, One-to-Many e Many-to-Many.

## Tour pelo código

No código abaixo, crio uma classe base para que as classes que a implementam possam utilizar o método **AtomicOperation**. Este método recebe como parâmetro uma callback contendo a operação que será realizada e envolve a operação dentro de um bloco Try-Catch. O método, por sua vez, já utiliza os comandos commit ou rollback para garantir a integridade dos dados. 

```cs
    public abstract class ConnectionFactoryBase : IConnectionFactory
    {
        protected readonly IDbConnection _connection;

        public IDbConnection Connection => _connection;

        private IDbTransaction? _transaction;

        public ConnectionFactoryBase(IDbConnection dbConnection) 
        {
            _connection = dbConnection;
            _connection.Open();
        }


        public async Task<T> AtomicOperation<T>(Func<IDbTransaction, Task<T>> callback)
        {
            _transaction = _connection.BeginTransaction();
            try 
            {   
                var result = callback(_transaction!);
                _transaction!.Commit();
                return await result;
            }
            catch 
            {
                _transaction!.Rollback();
                throw;
            } 
            finally 
            {
            _connection.Close(); 
            }
        }
    }
```

Classe que irá implementa a classe abstrata acima

```cs    
    public class SqliteConnectionFactory : ConnectionFactoryBase
    {
        public SqliteConnectionFactory() : base(new SqliteConnection("Data Source=app.db"))
        {}
    }
```

## Relacionamentos

No código abaixo eu faço um SQL que faz vários JUNÇÃO de algumas tabelas e o Dapper através de uma função callback mapeia os objetos para dentro da minha lista de usuarios. O Dapper faz o SplitOn no Id como padrão, ou seja, se o Id das tabelas forem diferentes no banco isso pode me gerar uma exception caso eu não especifique esse parâmetro para o método Query. Além disso, a ordem como é trazido os dados importam no genéric do **QueryAsync**.


```cs
    public async Task<Usuario?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
    {
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

```