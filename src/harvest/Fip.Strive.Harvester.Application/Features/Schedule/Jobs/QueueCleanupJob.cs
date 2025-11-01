using Fip.Strive.Harvester.Application.Core.Queue;
using Fip.Strive.Harvester.Application.Core.Queue.Repositories.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Fip.Strive.Harvester.Application.Features.Schedule.Jobs;

public class QueueCleanupJob(
    IJobDeleter deleter,
    ILogger<QueueCleanupJob> logger,
    IOptions<QueueConfig> config
) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        var configuredDeleteTime = DateTime.UtcNow.AddDays(config.Value.DeleteAfterDays * -1);
        var deleteCount = deleter.DeleteBefore(configuredDeleteTime);

        logger.LogInformation("Cleaned up queue, removed {QueueDeleteCount} jobs", deleteCount);

        return Task.CompletedTask;
    }
}
