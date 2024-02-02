using System.Data;

namespace DapperTesting.Factories;

public interface IConnectionFactory
{
   IDbConnection Connection { get; }
   Task<T> AtomicOperation<T>(Func<IDbTransaction, Task<T>> callback);
}