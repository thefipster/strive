using Fip.Strive.Harvester.Application.Core.Queue.Repositories.Contracts;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Fip.Strive.Harvester.Application.Features.Schedule.Jobs;

public class QueueCleanupJob(IJobDeleter deleter, ILogger<QueueCleanupJob> logger) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        var deleteCount = deleter.DeleteBefore(DateTime.UtcNow.AddMinutes(-10));
        //deleter.Rebuild();

        logger.LogInformation("Cleaned up queue, removed {QueueDeleteCount} jobs", deleteCount);

        return Task.CompletedTask;
    }
}
