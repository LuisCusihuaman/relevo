using Dapper;
using Relevo.UseCases.Contributors;
using Relevo.UseCases.Contributors.List;
using System.Data;

namespace Relevo.Infrastructure.Data.Queries;

public class OracleListContributorsQueryService(Data.Oracle.IOracleConnectionFactory _factory)
  : IListContributorsQueryService
{
  public async Task<IEnumerable<ContributorDTO>> ListAsync()
  {
    using IDbConnection conn = _factory.CreateConnection();
    const string sql = @"SELECT 
      c.Id AS Id,
      c.Name AS Name,
      c.PhoneNumber_Number AS PhoneNumber
    FROM Contributors c";

    var rows = await conn.QueryAsync<ContributorDTO>(sql);
    return rows;
  }
}


