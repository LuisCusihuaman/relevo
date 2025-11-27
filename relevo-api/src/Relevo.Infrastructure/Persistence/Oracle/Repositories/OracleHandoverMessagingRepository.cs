using System;
using System.Data;
using System.Collections.Generic;
using Dapper;
using Microsoft.Extensions.Logging;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data.Oracle;
using System;

namespace Relevo.Infrastructure.Persistence.Oracle.Repositories;

public class OracleHandoverMessagingRepository : IHandoverMessagingRepository
{
    private readonly IOracleConnectionFactory _factory;
    private readonly ILogger<OracleHandoverMessagingRepository> _logger;

    public OracleHandoverMessagingRepository(IOracleConnectionFactory factory, ILogger<OracleHandoverMessagingRepository> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public IReadOnlyList<HandoverMessageRecord> GetHandoverMessages(string handoverId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                SELECT hm.ID, hm.HANDOVER_ID as HandoverId, hm.USER_ID as UserId,
                       hm.USER_NAME as UserName,
                       hm.MESSAGE_TEXT as MessageText, hm.MESSAGE_TYPE as MessageType,
                       hm.CREATED_AT as CreatedAt, hm.UPDATED_AT as UpdatedAt
                FROM HANDOVER_MESSAGES hm
                WHERE hm.HANDOVER_ID = :handoverId
                ORDER BY hm.CREATED_AT ASC";

            return conn.Query<HandoverMessageRecord>(sql, new { handoverId }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get handover messages for handover {HandoverId}", handoverId);
            throw;
        }
    }

    public HandoverMessageRecord CreateHandoverMessage(string handoverId, string userId, string userName, string messageText, string messageType)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            var id = Guid.NewGuid().ToString();

            const string sql = @"
                INSERT INTO HANDOVER_MESSAGES (ID, HANDOVER_ID, USER_ID, USER_NAME, MESSAGE_TEXT, MESSAGE_TYPE, CREATED_AT, UPDATED_AT)
                VALUES (:id, :handoverId, :userId, :userName, :messageText, :messageType, SYSTIMESTAMP, SYSTIMESTAMP)";

            var parameters = new
            {
                id,
                handoverId,
                userId,
                userName,
                messageText,
                messageType
            };

            conn.Execute(sql, parameters);
            return new HandoverMessageRecord(id, handoverId, userId, userName, messageText, messageType, DateTime.Now, DateTime.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create handover message for handover {HandoverId}", handoverId);
            throw;
        }
    }
}
