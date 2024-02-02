using System.Data;
using Microsoft.Data.Sqlite;

namespace DapperTesting.Factories.Base;

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