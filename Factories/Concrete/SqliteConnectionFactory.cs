using System.Data;
using DapperTesting.Factories.Base;
using Microsoft.Data.Sqlite;

namespace DapperTesting.Factories.Concrete;

public class SqliteConnectionFactory : ConnectionFactoryBase
{
    protected override IDbConnection _connection => new SqliteConnection("Data Source=app.db");
}