using System;
using System.Data;
using System.Collections.Generic;
using Dapper;
using Microsoft.Extensions.Logging;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data.Oracle;

namespace Relevo.Infrastructure.Persistence.Oracle.Repositories;

public class OracleHandoverChecklistRepository : IHandoverChecklistRepository
{
    private readonly IOracleConnectionFactory _factory;
    private readonly ILogger<OracleHandoverChecklistRepository> _logger;

    public OracleHandoverChecklistRepository(IOracleConnectionFactory factory, ILogger<OracleHandoverChecklistRepository> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public IReadOnlyList<HandoverChecklistItemRecord> GetHandoverChecklists(string handoverId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                SELECT ID, HANDOVER_ID as HandoverId, USER_ID as UserId, ITEM_ID as ItemId,
                       ITEM_CATEGORY as ItemCategory, ITEM_LABEL as ItemLabel,
                       ITEM_DESCRIPTION as ItemDescription, IS_REQUIRED as IsRequired,
                       IS_CHECKED as IsChecked, CHECKED_AT as CheckedAt, CREATED_AT as CreatedAt
                FROM HANDOVER_CHECKLISTS
                WHERE HANDOVER_ID = :handoverId
                ORDER BY CREATED_AT ASC";

            return conn.Query<HandoverChecklistItemRecord>(sql, new { handoverId }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get handover checklists for handover {HandoverId}", handoverId);
            throw;
        }
    }

    public bool UpdateChecklistItem(string handoverId, string itemId, bool isChecked, string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                UPDATE HANDOVER_CHECKLISTS
                SET IS_CHECKED = :isChecked,
                    CHECKED_AT = CASE WHEN :isChecked = 1 THEN SYSTIMESTAMP ELSE NULL END,
                    UPDATED_AT = SYSTIMESTAMP
                WHERE HANDOVER_ID = :handoverId AND ITEM_ID = :itemId AND USER_ID = :userId";

            var result = conn.Execute(sql, new { handoverId, itemId, isChecked = isChecked ? 1 : 0, userId });
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update checklist item {ItemId} for handover {HandoverId}", itemId, handoverId);
            throw;
        }
    }
}
