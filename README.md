# Testando o Dapper

Neste projeto, desenvolvo uma pequena API com a abordagem minimalista, utilizando o Dapper como ORM para mapear as colunas para um objeto de destino. No contexto dessa implementação, faço uso de diferentes formas de relacionamento, como One-to-One, One-to-Many e Many-to-Many.

## Tour pelo código

No código abaixo, crio uma classe base para que as classes que a implementam possam utilizar o método **AtomicOperation**. Este método recebe como parâmetro uma callback contendo a operação que será realizada e envolve a operação dentro de um bloco Try-Catch. O método, por sua vez, já utiliza os comandos commit ou rollback para garantir a integridade dos dados. 

```cs
    public abstract class ConnectionFactoryBase : IConnectionFactory
    {
        protected abstract IDbConnection _connection { get;  }

        public IDbConnection Connection => _connection;

        private IDbTransaction? _transaction;

        public async Task<T> AtomicOperation<T>(Func<IDbTransaction, Task<T>> callback)
        {
            StartTransaction();
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
            Connection.Close(); 
            }
        }

        public void StartTransaction()
        {
            ArgumentNullException.ThrowIfNull(Connection);
            Connection.Open();
            _transaction = Connection.BeginTransaction();
        }
    }

```