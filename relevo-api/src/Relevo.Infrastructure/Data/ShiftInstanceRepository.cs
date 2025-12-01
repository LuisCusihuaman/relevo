using System.Data;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Data;

public class ShiftInstanceRepository(DapperConnectionFactory _connectionFactory) : IShiftInstanceRepository
{
    public async Task<string> GetOrCreateShiftInstanceAsync(string shiftId, string unitId, DateTime startAt, DateTime endAt)
    {
        using var conn = _connectionFactory.CreateConnection();

        // Try to get existing instance first
        const string getSql = @"
            SELECT ID 
            FROM SHIFT_INSTANCES 
            WHERE UNIT_ID = :unitId 
              AND SHIFT_ID = :shiftId 
              AND START_AT = :startAt";
        
        var existingId = await conn.ExecuteScalarAsync<string>(getSql, new { unitId, shiftId, startAt });
        
        if (!string.IsNullOrEmpty(existingId))
        {
            return existingId;
        }

        // Create new instance
        var id = $"si-{Guid.NewGuid().ToString()[..8]}";
        
        const string insertSql = @"
            INSERT INTO SHIFT_INSTANCES (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT, CREATED_AT, UPDATED_AT)
            VALUES (:id, :unitId, :shiftId, :startAt, :endAt, LOCALTIMESTAMP, LOCALTIMESTAMP)";
        
        try
        {
            await conn.ExecuteAsync(insertSql, new { id, unitId, shiftId, startAt, endAt });
            return id;
        }
        catch (OracleException ex) when (ex.Number == 1) // Unique constraint violation
        {
            // Race condition: another thread created it, fetch it
            return await conn.ExecuteScalarAsync<string>(getSql, new { unitId, shiftId, startAt }) ?? id;
        }
    }

    public async Task<ShiftInstanceRecord?> GetShiftInstanceByIdAsync(string shiftInstanceId)
    {
        using var conn = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT ID, UNIT_ID as UnitId, SHIFT_ID as ShiftId, 
                   START_AT as StartAt, END_AT as EndAt,
                   CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
            FROM SHIFT_INSTANCES
            WHERE ID = :shiftInstanceId";
        
        return await conn.QueryFirstOrDefaultAsync<ShiftInstanceRecord>(sql, new { shiftInstanceId });
    }

    public async Task<IReadOnlyList<ShiftInstanceRecord>> GetShiftInstancesAsync(string unitId, DateTime? startDate, DateTime? endDate)
    {
        using var conn = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT ID, UNIT_ID as UnitId, SHIFT_ID as ShiftId,
                   START_AT as StartAt, END_AT as EndAt,
                   CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
            FROM SHIFT_INSTANCES
            WHERE UNIT_ID = :unitId";
        
        object parameters;
        
        if (startDate.HasValue && endDate.HasValue)
        {
            sql += " AND START_AT >= :startDate AND START_AT <= :endDate";
            parameters = new { unitId, startDate = startDate.Value, endDate = endDate.Value };
        }
        else if (startDate.HasValue)
        {
            sql += " AND START_AT >= :startDate";
            parameters = new { unitId, startDate = startDate.Value };
        }
        else if (endDate.HasValue)
        {
            sql += " AND START_AT <= :endDate";
            parameters = new { unitId, endDate = endDate.Value };
        }
        else
        {
            parameters = new { unitId };
        }
        
        sql += " ORDER BY START_AT DESC";
        
        var instances = await conn.QueryAsync<ShiftInstanceRecord>(sql, parameters);
        return instances.ToList();
    }
}

