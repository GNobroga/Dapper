using System.Data;

namespace DapperTesting.Factories.Base;

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