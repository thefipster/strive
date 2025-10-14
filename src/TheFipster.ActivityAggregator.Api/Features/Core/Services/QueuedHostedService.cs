using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Hubs;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Configs;

namespace TheFipster.ActivityAggregator.Api.Features.Core.Services
{
    public class QueuedHostedService(
        IOptions<ApiConfig> config,
        IBackgroundTaskQueue queue,
        IHubContext<ImportHub> hubContext,
        ILogger<QueuedHostedService> logger
    ) : BackgroundService
    {
        private int _activeWorkers;
        private readonly ConcurrentQueue<DateTime> _completionTimes = new();
        private readonly TimeSpan _windowSize = TimeSpan.FromSeconds(10);
        private DateTime? _queueEmptyTime;
        private TimeSpan _processingRate = TimeSpan.FromMilliseconds(100);
        private TimeSpan _updateRate = TimeSpan.FromSeconds(1);

        protected override Task ExecuteAsync(CancellationToken ct)
        {
            var workers = Enumerable
                .Range(0, config.Value.MaxDegreeOfParallelism)
                .Select(workerId => Task.Run(() => WorkerLoop(workerId, ct), ct))
                .ToList();

            workers.Add(Task.Run(() => ReporterLoop(ct), ct));

            return Task.WhenAll(workers);
        }

        private async Task WorkerLoop(int workerId, CancellationToken ct)
        {
            logger.LogInformation("Worker {WorkerId} starting.", workerId);

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var workItem = await queue.DequeueAsync(ct);
                    Interlocked.Increment(ref _activeWorkers);
                    await workItem(ct);
                    RecordCompletion();
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
                    Interlocked.Decrement(ref _activeWorkers);
                    await Task.Delay(_processingRate, ct);
                }
            }

            logger.LogInformation("Worker {WorkerId} stopping.", workerId);
        }

        private async Task ReporterLoop(CancellationToken ct)
        {
            logger.LogInformation("Reporter starting.");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var rate = GetProcessingRate();

                    if (_completionTimes.Count == 0)
                    {
                        if (_queueEmptyTime == null)
                            _queueEmptyTime = DateTime.UtcNow;

                        if (DateTime.UtcNow - _queueEmptyTime > _windowSize)
                            continue;
                    }
                    else
                    {
                        _queueEmptyTime = null;
                    }

                    await hubContext
                        .Clients.Group(Const.Hubs.Importer.Actions.Queue)
                        .SendAsync(
                            Const.Hubs.Importer.ReportQueue,
                            queue.Count,
                            config.Value.MaxDegreeOfParallelism,
                            _activeWorkers,
                            rate,
                            ct
                        );
                }
                catch (OperationCanceledException)
                {
                    // Graceful shutdown
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred in reporter loop.");
                }
                finally
                {
                    await Task.Delay(_updateRate, ct);
                }
            }

            logger.LogInformation("Reporter stopping.");
        }

        private void RecordCompletion()
        {
            var now = DateTime.UtcNow;
            _completionTimes.Enqueue(now);

            // Clean up old timestamps
            while (_completionTimes.TryPeek(out var oldest) && now - oldest > _windowSize)
                _completionTimes.TryDequeue(out _);
        }

        private double GetProcessingRate()
        {
            var now = DateTime.UtcNow;

            // purge old entries
            while (_completionTimes.TryPeek(out var oldest) && now - oldest > _windowSize)
                _completionTimes.TryDequeue(out _);

            var count = _completionTimes.Count;
            var rate = count / _windowSize.TotalSeconds;

            return Math.Round(rate, 1);
        }
    }
}
