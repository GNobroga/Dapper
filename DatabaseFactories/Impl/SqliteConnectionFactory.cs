using System.Data;
using Microsoft.Data.Sqlite;

namespace DapperTesting.DatabaseFactories.Impl;

public class SqliteConnectionFactory : IConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        using var connection = new SqliteConnection("Data Source=app.db");
        return connection;
    }
}