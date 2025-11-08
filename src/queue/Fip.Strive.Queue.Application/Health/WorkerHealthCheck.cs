using Fip.Strive.Queue.Application.Services;
using Fip.Strive.Queue.Application.Services.Contracts;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Fip.Strive.Queue.Application.Health;

public class WorkerHealthCheck(IProcessingService queueService) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        var status = queueService.IsRunning;

        if (status)
            return Task.FromResult(HealthCheckResult.Healthy("Workers running."));

        return Task.FromResult(HealthCheckResult.Degraded("Workers stopped."));
    }
}
