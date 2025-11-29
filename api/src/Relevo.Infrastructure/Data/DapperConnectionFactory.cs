using System.Data;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace Relevo.Infrastructure.Data;

public class DapperConnectionFactory(IConfiguration _configuration)
{
  public IDbConnection CreateConnection()
  {
    // BindByName is set globally in CustomWebApplicationFactory for tests, 
    // and should be set in Program.cs for the app.
    // Do not set it here if connection might be already open elsewhere in the same process.
    return new OracleConnection(_configuration.GetConnectionString("OracleConnection"));
  }
}
