using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Relevo.Infrastructure.Data.Oracle;

namespace Relevo.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job that automatically expires handovers that are past their window date.
/// Runs on a schedule to mark old, non-accepted handovers as expired.
/// </summary>
public class ExpireHandoversJob
{
    private readonly IOracleConnectionFactory _factory;
    private readonly ILogger<ExpireHandoversJob> _logger;

    public ExpireHandoversJob(IOracleConnectionFactory factory, ILogger<ExpireHandoversJob> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    /// <summary>
    /// Executes the expiration job, marking eligible handovers as expired.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Number of handovers that were expired</returns>
    public async Task<int> ExecuteAsync(CancellationToken ct = default)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            const string sql = @"
                UPDATE HANDOVERS
                SET EXPIRED_AT = SYSTIMESTAMP,
                    STATUS = 'Expired',
                    UPDATED_AT = SYSTIMESTAMP
                WHERE HANDOVER_WINDOW_DATE < TRUNC(SYSDATE) - 1  -- Over 1 day old
                  AND COMPLETED_AT IS NULL                        -- Not completed
                  AND CANCELLED_AT IS NULL                        -- Not cancelled
                  AND REJECTED_AT IS NULL                         -- Not rejected
                  AND EXPIRED_AT IS NULL                          -- Not already expired
                  AND ACCEPTED_AT IS NULL                         -- Don't expire accepted handovers
                  AND STARTED_AT IS NULL                          -- Don't expire in-progress handovers";

            int affected = await conn.ExecuteAsync(sql);

            if (affected > 0)
            {
                _logger.LogInformation("Expired {Count} handover(s)", affected);
            }
            else
            {
                _logger.LogDebug("No handovers to expire");
            }

            return affected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute expiration job");
            throw;
        }
    }
}

