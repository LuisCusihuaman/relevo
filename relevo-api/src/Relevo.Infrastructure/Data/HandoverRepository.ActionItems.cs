using Dapper;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Data;

/// <summary>
/// Handover action items CRUD operations.
/// Manages the HANDOVER_ACTION_ITEMS table.
/// </summary>
public partial class HandoverRepository
{
    public async Task<IReadOnlyList<HandoverActionItemFullRecord>> GetActionItemsAsync(string handoverId)
    {
        using var conn = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT ID, HANDOVER_ID as HandoverId, DESCRIPTION, IS_COMPLETED as IsCompleted,
                   CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt, COMPLETED_AT as CompletedAt
            FROM HANDOVER_ACTION_ITEMS
            WHERE HANDOVER_ID = :handoverId
            ORDER BY CREATED_AT DESC";

        var items = await conn.QueryAsync<HandoverActionItemFullRecord>(sql, new { handoverId });
        return items.ToList();
    }

    public async Task<HandoverActionItemFullRecord> CreateActionItemAsync(string handoverId, string description, string priority)
    {
        using var conn = _connectionFactory.CreateConnection();
        var id = $"action-{Guid.NewGuid().ToString()[..8]}";
        var now = DateTime.UtcNow;

        const string sql = @"
            INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED, CREATED_AT, UPDATED_AT)
            VALUES (:id, :handoverId, :description, 0, LOCALTIMESTAMP, LOCALTIMESTAMP)";

        await conn.ExecuteAsync(sql, new { id, handoverId, description });

        return new HandoverActionItemFullRecord(
            id,
            handoverId,
            description,
            false,
            now,
            now,
            null
        );
    }

    public async Task<bool> UpdateActionItemAsync(string handoverId, string itemId, bool isCompleted)
    {
        using var conn = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE HANDOVER_ACTION_ITEMS
            SET IS_COMPLETED = :isCompleted,
                COMPLETED_AT = CASE WHEN :isCompleted = 1 THEN LOCALTIMESTAMP ELSE NULL END,
                UPDATED_AT = LOCALTIMESTAMP
            WHERE ID = :itemId AND HANDOVER_ID = :handoverId";

        var result = await conn.ExecuteAsync(sql, new { itemId, handoverId, isCompleted = isCompleted ? 1 : 0 });
        return result > 0;
    }

    public async Task<bool> DeleteActionItemAsync(string handoverId, string itemId)
    {
        using var conn = _connectionFactory.CreateConnection();

        const string sql = @"DELETE FROM HANDOVER_ACTION_ITEMS WHERE ID = :itemId AND HANDOVER_ID = :handoverId";

        var result = await conn.ExecuteAsync(sql, new { itemId, handoverId });
        return result > 0;
    }
}
