using System.Data;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace Relevo.Infrastructure.Data.Oracle;

public interface IOracleConnectionFactory
{
  IDbConnection CreateConnection();
}

public class OracleConnectionFactory(IConfiguration _configuration) : IOracleConnectionFactory
{
  public IDbConnection CreateConnection()
  {
    // Prefer explicit Oracle connection string, fallback to generic ConnectionStrings:Oracle
    string? conn = _configuration["Oracle:ConnectionString"]
                   ?? _configuration.GetConnectionString("Oracle");

    if (string.IsNullOrWhiteSpace(conn))
    {
      throw new InvalidOperationException("Oracle connection string not configured. Set Oracle:ConnectionString or ConnectionStrings:Oracle.");
    }

    return new OracleConnection(conn);
  }
}


