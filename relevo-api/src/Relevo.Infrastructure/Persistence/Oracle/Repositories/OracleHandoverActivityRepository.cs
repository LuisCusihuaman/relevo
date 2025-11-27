using System;
using System.Data;
using System.Collections.Generic;
using Dapper;
using Microsoft.Extensions.Logging;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data.Oracle;

namespace Relevo.Infrastructure.Persistence.Oracle.Repositories;

public class OracleHandoverActivityRepository : IHandoverActivityRepository
{
    private readonly IOracleConnectionFactory _factory;
    private readonly ILogger<OracleHandoverActivityRepository> _logger;

    public OracleHandoverActivityRepository(IOracleConnectionFactory factory, ILogger<OracleHandoverActivityRepository> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public IReadOnlyList<HandoverActivityItemRecord> GetHandoverActivityLog(string handoverId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                SELECT hal.ID, hal.HANDOVER_ID as HandoverId, hal.USER_ID as UserId,
                       u.FIRST_NAME || ' ' || u.LAST_NAME as UserName,
                       hal.ACTIVITY_TYPE as ActivityType, hal.ACTIVITY_DESCRIPTION as ActivityDescription,
                       hal.SECTION_AFFECTED as SectionAffected, hal.METADATA,
                       hal.CREATED_AT as CreatedAt
                FROM HANDOVER_ACTIVITY_LOG hal
                INNER JOIN USERS u ON hal.USER_ID = u.ID
                WHERE hal.HANDOVER_ID = :handoverId
                ORDER BY hal.CREATED_AT DESC";

            return conn.Query<HandoverActivityItemRecord>(sql, new { handoverId }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get handover activity log for handover {HandoverId}", handoverId);
            throw;
        }
    }
}
