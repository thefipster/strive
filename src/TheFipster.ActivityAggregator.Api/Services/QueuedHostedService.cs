using TheFipster.ActivityAggregator.Api.Abtraction;

namespace TheFipster.ActivityAggregator.Api.Services;

public class QueuedHostedService : BackgroundService
{
    private readonly IBackgroundTaskQueue taskQueue;
    private readonly ILogger<QueuedHostedService> logger;

    public QueuedHostedService(IBackgroundTaskQueue taskQueue, ILogger<QueuedHostedService> logger)
    {
        this.taskQueue = taskQueue;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await taskQueue.DequeueAsync(stoppingToken);

            try
            {
                await workItem(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred executing background job.");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}
