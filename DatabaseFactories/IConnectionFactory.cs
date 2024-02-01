using System.Data;

namespace DapperTesting.DatabaseFactories;

public interface IConnectionFactory 
{
    IDbConnection CreateConnection();
}