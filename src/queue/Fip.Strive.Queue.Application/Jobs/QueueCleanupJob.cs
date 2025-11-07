using Fip.Strive.Queue.Application.Repositories.Contracts;
using Fip.Strive.Queue.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Queue.Application.Jobs;

public class QueueCleanupJob(
    IJobDeleter deleter,
    ILogger<QueueCleanupJob> logger,
    IOptions<QueueConfig> config
)
{
    public Task Execute()
    {
        var configuredDeleteTime = DateTime.UtcNow.AddDays(config.Value.DeleteAfterDays * -1);
        var deleteCount = deleter.DeleteBefore(configuredDeleteTime);

        logger.LogInformation("Cleaned up queue, removed {QueueDeleteCount} jobs", deleteCount);

        return Task.CompletedTask;
    }
}
