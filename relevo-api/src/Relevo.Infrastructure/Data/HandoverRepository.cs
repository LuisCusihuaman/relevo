using System.Data;
using Dapper;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Data;

/// <summary>
/// Repository for Handover operations - Base class with constructor and private helpers.
/// Split into partial classes for maintainability:
/// - HandoverRepository.Queries.cs - Read operations
/// - HandoverRepository.StateMachine.cs - State transitions (Create, Ready, Start, Complete, Cancel)
/// - HandoverRepository.Contents.cs - Clinical data, Synthesis, Situation Awareness
/// - HandoverRepository.ActionItems.cs - Action items CRUD
/// - HandoverRepository.Messages.cs - Messages CRUD
/// - HandoverRepository.ContingencyPlans.cs - Contingency plans CRUD
/// </summary>
public partial class HandoverRepository(
    DapperConnectionFactory _connectionFactory,
    IShiftInstanceRepository _shiftInstanceRepository,
    IShiftWindowRepository _shiftWindowRepository) : IHandoverRepository
{
    private IShiftInstanceRepository ShiftInstanceRepository => _shiftInstanceRepository;
    private IShiftWindowRepository ShiftWindowRepository => _shiftWindowRepository;

    #region Private Helpers

    private async Task<PhysicianRecord> GetPhysicianInfo(IDbConnection conn, string userId, string handoverStatus, string relationship)
    {
        // Get Name
        var name = await conn.ExecuteScalarAsync<string>("SELECT FULL_NAME FROM USERS WHERE ID = :UserId", new { UserId = userId }) ?? "Unknown";
        
        // Get Shift from SHIFT_COVERAGE -> SHIFT_INSTANCES -> SHIFTS
        const string shiftSql = @"
            SELECT * FROM (
                SELECT s.START_TIME, s.END_TIME
                FROM SHIFT_COVERAGE sc
                JOIN SHIFT_INSTANCES si ON sc.SHIFT_INSTANCE_ID = si.ID
                JOIN SHIFTS s ON si.SHIFT_ID = s.ID
                WHERE sc.RESPONSIBLE_USER_ID = :UserId
                ORDER BY sc.ASSIGNED_AT DESC
            ) WHERE ROWNUM <= 1";
        
        var shift = await conn.QueryFirstOrDefaultAsync<dynamic>(shiftSql, new { UserId = userId });
        
        string status = CalculatePhysicianStatus(handoverStatus, relationship);

        return new PhysicianRecord(
            name,
            "Doctor",
            "", // Color
            (string?)shift?.START_TIME,
            (string?)shift?.END_TIME,
            status,
            relationship == "creator" ? "assigned" : "receiving"
        );
    }

    private async Task<bool> UpdateHandoverStatus(string handoverId, string status, string timestampColumn, string userId)
    {
        using var conn = _connectionFactory.CreateConnection();
        // CURRENT_STATE is virtual, calculated automatically from timestamp columns
        // Only update the timestamp column, not STATUS
        string sql = $@"
            UPDATE HANDOVERS
            SET {timestampColumn} = LOCALTIMESTAMP, 
                UPDATED_AT = LOCALTIMESTAMP
            WHERE ID = :handoverId";

        var rows = await conn.ExecuteAsync(sql, new { handoverId });
        return rows > 0;
    }

    private static string CalculatePhysicianStatus(string state, string relationship)
    {
        // V3: Only mechanical states exist (Draft, Ready, InProgress, Completed, Cancelled)
        // Rejected and Expired were removed - rejection is modeled as Cancelled with CANCEL_REASON='ReceiverRefused'
        state = state?.ToLower() ?? "";
        return state switch
        {
            "completed" => "completed",
            "cancelled" => "cancelled",
            "draft" => relationship == "creator" ? "handing-off" : "pending",
            "ready" => relationship == "creator" ? "handing-off" : "ready-to-receive",
            "inprogress" => relationship == "creator" ? "handing-off" : "receiving",
            _ => "unknown"
        };
    }

    #endregion
}
