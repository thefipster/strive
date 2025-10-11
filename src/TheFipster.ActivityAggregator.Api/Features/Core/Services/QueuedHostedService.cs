using TheFipster.ActivityAggregator.Api.Features.Core.Components.Contracts;

namespace TheFipster.ActivityAggregator.Api.Features.Core.Services
{
    public class QueuedHostedService : BackgroundService
    {
        private const int MaxDegreeOfParallelism = 4;

        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger<QueuedHostedService> _logger;

        public QueuedHostedService(
            IBackgroundTaskQueue taskQueue,
            ILogger<QueuedHostedService> logger
        )
        {
            this._taskQueue = taskQueue;
            this._logger = logger;
        }

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
            _logger.LogInformation("Worker {WorkerId} starting.", workerId);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                    try
                    {
                        await workItem(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error occurred executing background job on worker {WorkerId}.",
                            workerId
                        );
                    }
                }
                catch (OperationCanceledException)
                {
                    // Graceful shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in worker loop {WorkerId}.", workerId);
                }
                finally
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }

            _logger.LogInformation("Worker {WorkerId} stopping.", workerId);
        }
    }
}
