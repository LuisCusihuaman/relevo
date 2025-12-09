using Dapper;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Data;

/// <summary>
/// Handover messages CRUD operations.
/// Manages the HANDOVER_MESSAGES table.
/// </summary>
public partial class HandoverRepository
{
    public async Task<IReadOnlyList<HandoverMessageRecord>> GetMessagesAsync(string handoverId)
    {
        using var conn = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT m.ID, m.HANDOVER_ID as HandoverId, m.USER_ID as UserId,
                   COALESCE(u.FULL_NAME, u.FIRST_NAME || ' ' || u.LAST_NAME, 'Unknown') as UserName,
                   m.MESSAGE_TEXT as MessageText, m.MESSAGE_TYPE as MessageType,
                   m.CREATED_AT as CreatedAt, m.UPDATED_AT as UpdatedAt
            FROM HANDOVER_MESSAGES m
            LEFT JOIN USERS u ON m.USER_ID = u.ID
            WHERE m.HANDOVER_ID = :handoverId
            ORDER BY m.CREATED_AT ASC";

        var messages = await conn.QueryAsync<HandoverMessageRecord>(sql, new { handoverId });
        return messages.ToList();
    }

    public async Task<HandoverMessageRecord> CreateMessageAsync(string handoverId, string userId, string userName, string messageText, string messageType)
    {
        using var conn = _connectionFactory.CreateConnection();
        var id = Guid.NewGuid().ToString();
        var now = DateTime.UtcNow;

        const string sql = @"
            INSERT INTO HANDOVER_MESSAGES (ID, HANDOVER_ID, USER_ID, MESSAGE_TEXT, MESSAGE_TYPE, CREATED_AT, UPDATED_AT)
            VALUES (:id, :handoverId, :userId, :messageText, :messageType, LOCALTIMESTAMP, LOCALTIMESTAMP)";

        await conn.ExecuteAsync(sql, new { id, handoverId, userId, messageText, messageType });

        return new HandoverMessageRecord(id, handoverId, userId, userName, messageText, messageType, now, now);
    }
}
