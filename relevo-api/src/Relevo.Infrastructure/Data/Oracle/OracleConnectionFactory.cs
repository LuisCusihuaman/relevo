using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;

namespace Relevo.Infrastructure.Data.Oracle;

public interface IOracleConnectionFactory
{
  IDbConnection CreateConnection();
}

public class OracleConnectionFactory(IConfiguration _configuration, ILogger<OracleConnectionFactory> _logger) : IOracleConnectionFactory, IDbConnectionFactory
{
  public IDbConnection CreateConnection()
  {
    // Prefer explicit Oracle connection string, fallback to generic ConnectionStrings:Oracle
    string? conn = _configuration["Oracle:ConnectionString"]
                   ?? _configuration.GetConnectionString("Oracle");

    if (string.IsNullOrWhiteSpace(conn))
    {
      _logger.LogError("Oracle connection string not configured. Set Oracle:ConnectionString or ConnectionStrings:Oracle.");
      throw new InvalidOperationException("Oracle connection string not configured. Set Oracle:ConnectionString or ConnectionStrings:Oracle.");
    }

    _logger.LogInformation("Creating Oracle connection to: {ConnectionString}", conn.Replace("Password=TuPass123", "Password=***"));

    try
    {
      var connection = new OracleConnection(conn);
      _logger.LogInformation("Oracle connection created successfully");
      return connection;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to create Oracle connection. Make sure Oracle database is running and accessible.");
      throw new InvalidOperationException("Failed to create Oracle connection. Make sure Oracle database is running and accessible.", ex);
    }
  }
}


