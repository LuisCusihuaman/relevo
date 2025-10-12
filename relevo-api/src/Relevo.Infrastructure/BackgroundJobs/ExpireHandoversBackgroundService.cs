using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Relevo.Infrastructure.BackgroundJobs;

/// <summary>
/// Hosted background service that runs the expiration job on a periodic schedule.
/// Runs every hour to check for and expire old handovers.
/// </summary>
public class ExpireHandoversBackgroundService : BackgroundService
{
    private readonly ILogger<ExpireHandoversBackgroundService> _logger;
    private readonly ExpireHandoversJob _job;
    private readonly TimeSpan _interval;

    public ExpireHandoversBackgroundService(
        ExpireHandoversJob job,
        ILogger<ExpireHandoversBackgroundService> logger)
    {
        _job = job;
        _logger = logger;
        _interval = TimeSpan.FromHours(1); // Run every hour
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Expiration background service starting");

        // Wait a bit before first run
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("Running handover expiration job");
                var expiredCount = await _job.ExecuteAsync(stoppingToken);
                
                if (expiredCount > 0)
                {
                    _logger.LogInformation("Expiration job completed: {Count} handover(s) expired", expiredCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running expiration job");
            }

            // Wait for next interval
            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Expiration background service stopping");
    }
}

