using Relevo.Core.ContributorAggregate;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Configuration;
using System.Data;
using Xunit;
using System;

namespace Relevo.IntegrationTests.Data;

public abstract class BaseDapperTestFixture : IDisposable
{
  protected IDbConnection? _connection;
  protected IDbTransaction? _transaction;
  protected string? _oracleUnavailableMessage;

  protected BaseDapperTestFixture()
  {
    try
    {
      _connection = CreateNewConnection();

      // Ensure the Contributors table exists with Oracle syntax
      EnsureTableExists();

      // Start a transaction for test isolation
      if (_connection != null)
      {
        _transaction = _connection.BeginTransaction();
      }

      // Clean up any existing test data within the transaction
      CleanTestData();
    }
    catch (Exception ex)
    {
      // If we can't connect to Oracle, set connection to null
      // Tests will handle this gracefully
      _connection = null;
      _transaction = null;
      _oracleUnavailableMessage = $"Oracle database not available: {ex.Message}";
    }
  }

  protected void EnsureTableExists()
  {
    if (_connection == null) return;

    try
    {
      using var cmd = _connection.CreateCommand();
      cmd.CommandText = "SELECT 1 FROM CONTRIBUTORS WHERE ROWNUM = 1";
      cmd.ExecuteScalar();
    }
    catch
    {
      // Table doesn't exist, create it
      try
      {
        // Create sequence first
        using var seqCmd = _connection.CreateCommand();
        seqCmd.CommandText = "CREATE SEQUENCE CONTRIBUTORS_SEQ START WITH 1 INCREMENT BY 1";
        seqCmd.ExecuteNonQuery();

        // Create table
        using var tableCmd = _connection.CreateCommand();
        tableCmd.CommandText = @"
          CREATE TABLE CONTRIBUTORS (
            ID NUMBER PRIMARY KEY,
            NAME VARCHAR2(200) NOT NULL,
            EMAIL VARCHAR2(200),
            PHONE_NUMBER VARCHAR2(20)
          )";
        tableCmd.ExecuteNonQuery();
      }
      catch (Exception ex)
      {
        // If we can't create the table/sequence, set error message
        _oracleUnavailableMessage = $"Cannot create test table/sequence: {ex.Message}";
      }
    }
  }

  protected void CleanTestData()
  {
    if (_connection == null) return;

    try
    {
      using var cmd = _connection.CreateCommand();
      if (_transaction != null)
      {
        cmd.Transaction = _transaction;
      }
      cmd.CommandText = "DELETE FROM CONTRIBUTORS";
      cmd.ExecuteNonQuery();
    }
    catch
    {
      // Ignore cleanup errors
    }
  }

  protected static IDbConnection CreateNewConnection()
  {
    // Use Oracle connection for integration tests
    var connection = new OracleConnection("User Id=system;Password=TuPass123;Data Source=localhost:1521/XE;Pooling=true;Connection Timeout=15");
    connection.Open();
    return connection;
  }


  public void Dispose()
  {
    // Rollback transaction to ensure test isolation
    _transaction?.Rollback();
    _transaction?.Dispose();
    _connection?.Dispose();
  }

  private class TestOracleConnectionFactory : IDbConnectionFactory
  {
    private readonly IDbConnection _testConnection;

    public TestOracleConnectionFactory(IDbConnection connection)
    {
      _testConnection = connection;
    }

    public IDbConnection CreateConnection()
    {
      return _testConnection;
    }
  }
}
