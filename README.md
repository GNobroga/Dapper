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

## Multiplo ResultSets

É possível fazer múltiplas consultas no Dapper usando o **QueryMultiple**. Muito útil pra criar relatórios.

```sql
    SELECT * FROM Usuarios WHERE Id = @Id;
    SELECT * FROM Contatos WHERE UsuarioId = @Id;
    SELECT * FROM EnderecosEntrega WHERE UsuarioId = @Id;
    SELECT D.* FROM UsuariosDepartamentos UD INNER JOIN Departamentos D ON UD.Departamento.Id = D.Id WHERE UD.UsuarioId = @Id;
```

```cs
   var resultSets = await _connection.QueryMultipleAsync(sql, new { Id = 1 });
   var usuario = resultSets.Read<Usuario>().SingleOrDefault();
   var contato = resultSets.Read<Contato>().SingleOrDefault();
   var enderecosEntrega = resultSets.Read<EnderecosEntrega>().ToList();
   var departamentos = resultSets.Read<Departamentos>().ToList();
```


## Utilizando Stored Procedure

É possível executar Store Procedure, é possível passar parâmetros também.

```cs
    _connection.Query<Usuario>("StoreProcedureName", commandType: CommandType.StoredProcedure);
```
## Biblioteca FluentMap

Permite mapear colunas com nomes diferentes das propriedades, pois o Dapper ele mapeia as colunas se baseando no nome da Propriedade, independente se estiver LowerCase, UpperCase, etc.

```cs
    var usuarios = _connection.Query<Usuario>("SELECT * FROM Usuarios;");
```

```cs
    public class UsuarioMap : EntityMap<Usuario>
    {
        public UsuarioMap()
        {
            Map(x => x.Property).ToColumn("ColumnName");
        }
    }

    FluentMapper.Initialize(config => config.AddMap(new UsuarioMap()));

```


## Helpful Packages

. Dapper.Contrib - Pacote que extende o Dapper com operações de um CRUD básico.