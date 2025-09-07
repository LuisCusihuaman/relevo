using Relevo.Core.ContributorAggregate;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Relevo.IntegrationTests.Data;

public abstract class BaseDapperTestFixture : IDisposable
{
  protected IDbConnection _connection;

  protected BaseDapperTestFixture()
  {
    _connection = CreateNewConnection();

    // Create the Contributors table
    using var cmd = _connection.CreateCommand();
    cmd.CommandText = @"
      CREATE TABLE Contributors (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Name TEXT NOT NULL,
        PhoneNumber_CountryCode TEXT,
        PhoneNumber_Number TEXT,
        PhoneNumber_Extension TEXT,
        Status INTEGER NOT NULL DEFAULT 0
      )";
    cmd.ExecuteNonQuery();
  }

  protected void EnsureTableExists()
  {
    try
    {
      using var cmd = _connection.CreateCommand();
      cmd.CommandText = "SELECT 1 FROM Contributors LIMIT 1";
      cmd.ExecuteScalar();
    }
    catch
    {
      // Table doesn't exist, create it
      using var cmd = _connection.CreateCommand();
      cmd.CommandText = @"
        CREATE TABLE Contributors (
          Id INTEGER PRIMARY KEY AUTOINCREMENT,
          Name TEXT NOT NULL,
          PhoneNumber_CountryCode TEXT,
          PhoneNumber_Number TEXT,
          PhoneNumber_Extension TEXT,
          Status INTEGER NOT NULL DEFAULT 0
        )";
      cmd.ExecuteNonQuery();
    }
  }

  protected static IDbConnection CreateNewConnection()
  {
    var connection = new SqliteConnection("Data Source=:memory:");
    connection.Open();
    return connection;
  }


  public void Dispose()
  {
    _connection?.Dispose();
  }

  private class TestSqliteConnectionFactory : IDbConnectionFactory
  {
    private readonly IDbConnection _testConnection;

    public TestSqliteConnectionFactory(IDbConnection connection)
    {
      _testConnection = connection;
    }

    public IDbConnection CreateConnection()
    {
      return _testConnection;
    }
  }
}
