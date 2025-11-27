using System;
using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data.Oracle;

namespace Relevo.Infrastructure.Persistence.Oracle.Repositories;

public class OracleHandoverSyncStatusRepository : IHandoverSyncStatusRepository
{
    private readonly IOracleConnectionFactory _factory;
    private readonly ILogger<OracleHandoverSyncStatusRepository> _logger;

    public OracleHandoverSyncStatusRepository(IOracleConnectionFactory factory, ILogger<OracleHandoverSyncStatusRepository> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public HandoverSyncStatusRecord? GetHandoverSyncStatus(string handoverId, string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            const string sql = @"
                SELECT ID, HANDOVER_ID, USER_ID, SYNC_STATUS, LAST_SYNC, VERSION
                FROM HANDOVER_SYNC_STATUS
                WHERE HANDOVER_ID = :handoverId AND USER_ID = :userId";

            var syncStatus = conn.QueryFirstOrDefault<HandoverSyncStatusRecord>(sql, new { handoverId, userId });

            // If no sync status exists, create a default one
            if (syncStatus == null)
            {
                syncStatus = new HandoverSyncStatusRecord(
                    Id: $"sync-{handoverId}-{userId}",
                    HandoverId: handoverId,
                    UserId: userId,
                    SyncStatus: "synced",
                    LastSync: DateTime.Now,
                    Version: 1
                );
            }

            return syncStatus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get sync status for handover {HandoverId}, user {UserId}", handoverId, userId);
            return null;
        }
    }
}
