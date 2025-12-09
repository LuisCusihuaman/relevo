using Dapper;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Data;

/// <summary>
/// Handover contingency plans CRUD operations.
/// Manages the HANDOVER_CONTINGENCY table.
/// </summary>
public partial class HandoverRepository
{
    public async Task<IReadOnlyList<ContingencyPlanRecord>> GetContingencyPlansAsync(string handoverId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT ID, HANDOVER_ID as HandoverId, CONDITION_TEXT as ConditionText,
                   ACTION_TEXT as ActionText, PRIORITY, STATUS, CREATED_BY as CreatedBy,
                   CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
            FROM HANDOVER_CONTINGENCY
            WHERE HANDOVER_ID = :handoverId
            ORDER BY CREATED_AT ASC";

        var plans = await conn.QueryAsync<ContingencyPlanRecord>(sql, new { handoverId });
        return plans.ToList();
    }

    public async Task<ContingencyPlanRecord> CreateContingencyPlanAsync(string handoverId, string condition, string action, string priority, string createdBy)
    {
        using var conn = _connectionFactory.CreateConnection();
        var id = Guid.NewGuid().ToString();

        const string sql = @"
            INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT, UPDATED_AT)
            VALUES (:id, :handoverId, :condition, :action, :priority, 'active', :createdBy, LOCALTIMESTAMP, LOCALTIMESTAMP)";

        await conn.ExecuteAsync(sql, new { id, handoverId, condition, action, priority, createdBy });

        return new ContingencyPlanRecord(
            id, handoverId, condition, action, priority, "active",
            createdBy, DateTime.UtcNow, DateTime.UtcNow);
    }

    public async Task<bool> DeleteContingencyPlanAsync(string handoverId, string contingencyId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            DELETE FROM HANDOVER_CONTINGENCY
            WHERE ID = :contingencyId AND HANDOVER_ID = :handoverId";

        var rows = await conn.ExecuteAsync(sql, new { contingencyId, handoverId });
        return rows > 0;
    }
}
