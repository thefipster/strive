using Fip.Strive.Harvester.Application.Core.Queue.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Fip.Strive.Harvester.Application.Core.Queue.Health;

public class WorkerHealthCheck : IHealthCheck
{
    private readonly QueuedHostedService _queueService;

    public WorkerHealthCheck(QueuedHostedService queueService)
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
            return Task.FromResult(HealthCheckResult.Healthy("Workers running."));

        return Task.FromResult(HealthCheckResult.Degraded("Workers stopped."));
    }
}
