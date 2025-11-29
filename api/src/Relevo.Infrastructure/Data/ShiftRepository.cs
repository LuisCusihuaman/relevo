using System.Data;
using Dapper;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Data;

public class ShiftRepository(DapperConnectionFactory _connectionFactory) : IShiftRepository
{
  public async Task<IReadOnlyList<ShiftRecord>> GetShiftsAsync()
  {
    using var conn = _connectionFactory.CreateConnection();
    const string sql = @"SELECT ID, NAME, START_TIME as StartTime, END_TIME as EndTime FROM SHIFTS ORDER BY ID";
    var shifts = await conn.QueryAsync<ShiftRecord>(sql);
    return shifts.ToList();
  }
}

