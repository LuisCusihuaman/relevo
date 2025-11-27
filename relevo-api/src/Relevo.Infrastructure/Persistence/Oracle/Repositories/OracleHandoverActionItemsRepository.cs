using System;
using System.Data;
using System.Collections.Generic;
using Dapper;
using Microsoft.Extensions.Logging;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data.Oracle;

namespace Relevo.Infrastructure.Persistence.Oracle.Repositories;

public class OracleHandoverActionItemsRepository : IHandoverActionItemsRepository
{
    private readonly IOracleConnectionFactory _factory;
    private readonly ILogger<OracleHandoverActionItemsRepository> _logger;

    public OracleHandoverActionItemsRepository(IOracleConnectionFactory factory, ILogger<OracleHandoverActionItemsRepository> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public IReadOnlyList<HandoverActionItemRecord> GetHandoverActionItems(string handoverId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                SELECT ID, HANDOVER_ID as HandoverId, DESCRIPTION, IS_COMPLETED as IsCompleted,
                       CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt, COMPLETED_AT as CompletedAt
                FROM HANDOVER_ACTION_ITEMS
                WHERE HANDOVER_ID = :handoverId
                ORDER BY CREATED_AT DESC";

            return conn.Query<HandoverActionItemRecord>(sql, new { handoverId }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get action items for handover {HandoverId}", handoverId);
            return Array.Empty<HandoverActionItemRecord>();
        }
    }

    public string CreateHandoverActionItem(string handoverId, string description, string priority)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            var id = $"action-{Guid.NewGuid().ToString().Substring(0, 8)}";

            const string sql = @"
                INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED, CREATED_AT, UPDATED_AT)
                VALUES (:id, :handoverId, :description, 0, SYSTIMESTAMP, SYSTIMESTAMP)";

            conn.Execute(sql, new { id, handoverId, description });
            return id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create action item for handover {HandoverId}", handoverId);
            throw;
        }
    }

    public bool UpdateHandoverActionItem(string handoverId, string itemId, bool isCompleted)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            const string sql = @"
                UPDATE HANDOVER_ACTION_ITEMS
                SET IS_COMPLETED = :isCompleted,
                    COMPLETED_AT = CASE WHEN :isCompleted = 1 THEN SYSTIMESTAMP ELSE NULL END,
                    UPDATED_AT = SYSTIMESTAMP
                WHERE ID = :itemId AND HANDOVER_ID = :handoverId";

            var result = conn.Execute(sql, new { itemId, handoverId, isCompleted = isCompleted ? 1 : 0 });
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update action item {ItemId} for handover {HandoverId}", itemId, handoverId);
            throw;
        }
    }

    public bool DeleteHandoverActionItem(string handoverId, string itemId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            const string sql = @"DELETE FROM HANDOVER_ACTION_ITEMS WHERE ID = :itemId AND HANDOVER_ID = :handoverId";

            var result = conn.Execute(sql, new { itemId, handoverId });
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete action item {ItemId} for handover {HandoverId}", itemId, handoverId);
            throw;
        }
    }
}
