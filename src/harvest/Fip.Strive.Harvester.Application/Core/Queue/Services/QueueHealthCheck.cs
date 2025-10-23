using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Fip.Strive.Harvester.Application.Core.Queue.Services;

public class QueueHealthCheck : IHealthCheck
{
    private readonly QueuedHostedService _queueService;

    public QueueHealthCheck(QueuedHostedService queueService)
    {
        _queueService = queueService;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        var status = _queueService.IsRunning;

        if (status)
            return Task.FromResult(HealthCheckResult.Healthy($"Queue is running."));

        return Task.FromResult(HealthCheckResult.Degraded($"Queue is stopped."));
    }
}
