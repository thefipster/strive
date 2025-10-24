using Fip.Strive.Harvester.Application.Core.Queue.Models;
using Fip.Strive.Harvester.Application.Infrastructure.Contexts;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Fip.Strive.Harvester.Application.Core.Queue.Health;

public class JobHealthCheck(SignalQueueContext queueContext) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var col = queueContext.GetCollectionNames();
            if (col.Contains(typeof(JobDetails).Name))
                return Task.FromResult(HealthCheckResult.Healthy("Jobs storage ok."));

            return Task.FromResult(HealthCheckResult.Degraded("Jobs storage missing."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Jobs not accessible.", ex));
        }
    }
}