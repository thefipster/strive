using TheFipster.ActivityAggregator.Api.Features.Core.Components.Contracts;

namespace TheFipster.ActivityAggregator.Api.Features.Core.Services
{
    public class QueuedHostedService(
        IBackgroundTaskQueue taskQueue,
        ILogger<QueuedHostedService> logger
    ) : BackgroundService
    {
        private const int MaxDegreeOfParallelism = 4;

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var workers = Enumerable
                .Range(0, MaxDegreeOfParallelism)
                .Select(workerId =>
                    Task.Run(() => WorkerLoop(workerId, stoppingToken), stoppingToken)
                )
                .ToArray();

            return Task.WhenAll(workers);
        }

        private async Task WorkerLoop(int workerId, CancellationToken stoppingToken)
        {
            logger.LogInformation("Worker {WorkerId} starting.", workerId);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var workItem = await taskQueue.DequeueAsync(stoppingToken);
                    await workItem(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Graceful shutdown
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred in worker loop {WorkerId}.", workerId);
                }
                finally
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }

            logger.LogInformation("Worker {WorkerId} stopping.", workerId);
        }
    }
}
