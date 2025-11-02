using Fip.Strive.Harvester.Application.Core.Queue.Repositories.Contracts;
using Fip.Strive.Harvester.Application.Core.Schedule;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Fip.Strive.Harvester.Application.Core.Queue.Jobs;

[RegularIntervalJob(60)]
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
