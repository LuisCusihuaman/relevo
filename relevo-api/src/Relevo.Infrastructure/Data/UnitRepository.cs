using System.Data;
using Dapper;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Data;

public class UnitRepository(DapperConnectionFactory _connectionFactory) : IUnitRepository
{
  public async Task<IReadOnlyList<UnitRecord>> GetUnitsAsync()
  {
    using var conn = _connectionFactory.CreateConnection();
    const string sql = "SELECT ID as Id, NAME as Name FROM UNITS ORDER BY ID";
    var units = await conn.QueryAsync<UnitRecord>(sql);
    return units.ToList();
  }
}

