using System.Data;
using DapperTesting.Factories.Base;
using Microsoft.Data.Sqlite;

namespace DapperTesting.Factories.Concrete;

public class SqliteConnectionFactory : ConnectionFactoryBase
{
    public SqliteConnectionFactory() : base(new SqliteConnection("Data Source=app.db"))
    {}

}