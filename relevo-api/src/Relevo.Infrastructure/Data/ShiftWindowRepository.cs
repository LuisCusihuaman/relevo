using System.Data;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Data;

public class ShiftWindowRepository(DapperConnectionFactory _connectionFactory) : IShiftWindowRepository
{
    public async Task<string> GetOrCreateShiftWindowAsync(string fromShiftInstanceId, string toShiftInstanceId, string unitId)
    {
        using var conn = _connectionFactory.CreateConnection();

        // Try to get existing window first
        const string getSql = @"
            SELECT ID 
            FROM SHIFT_WINDOWS 
            WHERE FROM_SHIFT_INSTANCE_ID = :fromShiftInstanceId 
              AND TO_SHIFT_INSTANCE_ID = :toShiftInstanceId";
        
        var existingId = await conn.ExecuteScalarAsync<string>(getSql, new { fromShiftInstanceId, toShiftInstanceId });
        
        if (!string.IsNullOrEmpty(existingId))
        {
            return existingId;
        }

        // Create new window
        var id = $"sw-{Guid.NewGuid().ToString()[..8]}";
        
        const string insertSql = @"
            INSERT INTO SHIFT_WINDOWS (ID, UNIT_ID, FROM_SHIFT_INSTANCE_ID, TO_SHIFT_INSTANCE_ID, CREATED_AT, UPDATED_AT)
            VALUES (:id, :unitId, :fromShiftInstanceId, :toShiftInstanceId, LOCALTIMESTAMP, LOCALTIMESTAMP)";
        
        try
        {
            await conn.ExecuteAsync(insertSql, new { id, unitId, fromShiftInstanceId, toShiftInstanceId });
            return id;
        }
        catch (OracleException ex) when (ex.Number == 1) // Unique constraint violation
        {
            // Race condition: another thread created it, fetch it
            return await conn.ExecuteScalarAsync<string>(getSql, new { fromShiftInstanceId, toShiftInstanceId }) ?? id;
        }
    }

    public async Task<ShiftWindowRecord?> GetShiftWindowByIdAsync(string shiftWindowId)
    {
        using var conn = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT ID, UNIT_ID as UnitId, FROM_SHIFT_INSTANCE_ID as FromShiftInstanceId, 
                   TO_SHIFT_INSTANCE_ID as ToShiftInstanceId,
                   CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
            FROM SHIFT_WINDOWS
            WHERE ID = :shiftWindowId";
        
        return await conn.QueryFirstOrDefaultAsync<ShiftWindowRecord>(sql, new { shiftWindowId });
    }

    public async Task<IReadOnlyList<ShiftWindowRecord>> GetShiftWindowsAsync(string unitId, DateTime? startDate, DateTime? endDate)
    {
        using var conn = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT sw.ID, sw.UNIT_ID as UnitId, sw.FROM_SHIFT_INSTANCE_ID as FromShiftInstanceId,
                   sw.TO_SHIFT_INSTANCE_ID as ToShiftInstanceId,
                   sw.CREATED_AT as CreatedAt, sw.UPDATED_AT as UpdatedAt
            FROM SHIFT_WINDOWS sw
            JOIN SHIFT_INSTANCES si ON sw.FROM_SHIFT_INSTANCE_ID = si.ID
            WHERE sw.UNIT_ID = :unitId";
        
        object parameters;
        
        if (startDate.HasValue && endDate.HasValue)
        {
            sql += " AND si.START_AT >= :startDate AND si.START_AT <= :endDate";
            parameters = new { unitId, startDate = startDate.Value, endDate = endDate.Value };
        }
        else if (startDate.HasValue)
        {
            sql += " AND si.START_AT >= :startDate";
            parameters = new { unitId, startDate = startDate.Value };
        }
        else if (endDate.HasValue)
        {
            sql += " AND si.START_AT <= :endDate";
            parameters = new { unitId, endDate = endDate.Value };
        }
        else
        {
            parameters = new { unitId };
        }
        
        sql += " ORDER BY si.START_AT DESC";
        
        var windows = await conn.QueryAsync<ShiftWindowRecord>(sql, parameters);
        return windows.ToList();
    }
}

