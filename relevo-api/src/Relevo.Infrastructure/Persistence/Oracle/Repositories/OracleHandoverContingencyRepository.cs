using System;
using System.Data;
using System.Collections.Generic;
using Dapper;
using Microsoft.Extensions.Logging;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data.Oracle;

namespace Relevo.Infrastructure.Persistence.Oracle.Repositories;

public class OracleHandoverContingencyRepository : IHandoverContingencyRepository
{
    private readonly IOracleConnectionFactory _factory;
    private readonly ILogger<OracleHandoverContingencyRepository> _logger;

    public OracleHandoverContingencyRepository(IOracleConnectionFactory factory, ILogger<OracleHandoverContingencyRepository> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public IReadOnlyList<HandoverContingencyPlanRecord> GetHandoverContingencyPlans(string handoverId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                SELECT ID, HANDOVER_ID as HandoverId, CONDITION_TEXT as ConditionText,
                       ACTION_TEXT as ActionText, PRIORITY, STATUS, CREATED_BY as CreatedBy,
                       CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
                FROM HANDOVER_CONTINGENCY
                WHERE HANDOVER_ID = :handoverId
                ORDER BY CREATED_AT ASC";

            return conn.Query<HandoverContingencyPlanRecord>(sql, new { handoverId }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get handover contingency plans for handover {HandoverId}", handoverId);
            throw;
        }
    }

    public HandoverContingencyPlanRecord CreateContingencyPlan(string handoverId, string conditionText, string actionText, string priority, string createdBy)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            var id = Guid.NewGuid().ToString();

            const string sql = @"
                INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT, UPDATED_AT)
                VALUES (:id, :handoverId, :conditionText, :actionText, :priority, 'active', :createdBy, SYSTIMESTAMP, SYSTIMESTAMP)";

            conn.Execute(sql, new { id, handoverId, conditionText, actionText, priority, createdBy });

            return new HandoverContingencyPlanRecord(
                id, handoverId, conditionText, actionText, priority, "active",
                createdBy, DateTime.Now, DateTime.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create contingency plan for handover {HandoverId}", handoverId);
            throw;
        }
    }

    public bool DeleteContingencyPlan(string handoverId, string contingencyId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                DELETE FROM HANDOVER_CONTINGENCY
                WHERE ID = :contingencyId AND HANDOVER_ID = :handoverId";

            var result = conn.Execute(sql, new { contingencyId, handoverId });
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete contingency plan {ContingencyId}", contingencyId);
            throw;
        }
    }
}
