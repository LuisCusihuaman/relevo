using Dapper;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data.Oracle;

namespace Relevo.Infrastructure.Repositories;

/// <summary>
/// Oracle implementation of the physician shift repository
/// </summary>
public class OraclePhysicianShiftRepository(IOracleConnectionFactory connectionFactory) : IPhysicianShiftRepository
{
    private readonly IOracleConnectionFactory _connectionFactory = connectionFactory;

    public async Task<string?> GetPhysicianShiftIdAsync(string userId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "SELECT SHIFT_ID FROM USER_ASSIGNMENTS WHERE USER_ID = :userId FETCH FIRST 1 ROWS ONLY";

        return await connection.ExecuteScalarAsync<string?>(sql, new { userId });
    }

    public async Task<(string? startTime, string? endTime)> GetShiftTimesAsync(string shiftId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "SELECT START_TIME, END_TIME FROM SHIFTS WHERE ID = :shiftId";

        var result = await connection.QuerySingleOrDefaultAsync<(string? startTime, string? endTime)>(sql, new { shiftId });

        return result;
    }
}
