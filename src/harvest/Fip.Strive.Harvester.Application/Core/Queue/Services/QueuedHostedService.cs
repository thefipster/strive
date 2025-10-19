using System.Collections.Concurrent;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Core.Queue.Services;

public class QueuedHostedService(
    IOptions<QueueConfig> config,
    ISignalQueue queue,
    IEnumerable<ISignalQueueWorker> workers,
    ILogger<QueuedHostedService> logger
) : BackgroundService
{
    private readonly TimeSpan _windowSize = TimeSpan.FromSeconds(config.Value.RateWindowSeconds);
    private readonly TimeSpan _processingDelay = TimeSpan.FromMilliseconds(
        config.Value.ProcessingDelayMs
    );
    private readonly TimeSpan _updateDelay = TimeSpan.FromMilliseconds(config.Value.UpdateDelayMs);
    private readonly ConcurrentQueue<DateTime> _completionTimes = new();

    private DateTime? _queueEmptyTime;
    private int _activeWorkers;

    protected override Task ExecuteAsync(CancellationToken ct)
    {
        var instances = Enumerable
            .Range(0, config.Value.MaxDegreeOfParallelism)
            .Select(workerId => Task.Run(() => WorkerLoop(workerId, ct), ct))
            .ToList();

        instances.Add(Task.Run(() => ReporterLoop(ct), ct));

        return Task.WhenAll(instances);
    }

    private async Task WorkerLoop(int workerId, CancellationToken ct)
    {
        logger.LogInformation("Worker {WorkerId} starting.", workerId);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var job = await queue.DequeueAsync(ct);
                if (job != null)
                {
                    Interlocked.Increment(ref _activeWorkers);
                    await DoAsync(job, ct);
                    RecordCompletion();
                }
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
                await Task.Delay(_processingDelay, ct);
                Interlocked.Decrement(ref _activeWorkers);
            }
        }

        logger.LogInformation("Worker {WorkerId} stopping.", workerId);
    }

    private async Task DoAsync(JobEntity job, CancellationToken ct)
    {
        var type = job.Type;
        var worker = workers.FirstOrDefault(worker => worker.Type == type);

        if (worker != null)
            await worker.ProcessAsync(job, ct);
    }

    private async Task ReporterLoop(CancellationToken ct)
    {
        logger.LogInformation("Reporter starting.");
        int lastCount = 0;
        bool finalized = false;

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var count = queue.Count;
                if (count != 0 || lastCount != 0)
                {
                    finalized = false;
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

                    // await hubContext
                    //     .Clients.Group(Const.Hubs.Importer.Actions.Queue)
                    //     .SendAsync(
                    //         Const.Hubs.Importer.ReportQueue,
                    //         queue.Count,
                    //         config.Value.MaxDegreeOfParallelism,
                    //         _activeWorkers,
                    //         rate,
                    //         ct
                    //     );
                }
                else
                {
                    if (!finalized)
                    {
                        finalized = true;
                        // await hubContext
                        //     .Clients.Group(Const.Hubs.Importer.Actions.Queue)
                        //     .SendAsync(
                        //         Const.Hubs.Importer.ReportQueue,
                        //         0,
                        //         config.Value.MaxDegreeOfParallelism,
                        //         0,
                        //         0,
                        //         ct
                        //     );
                    }
                }

                lastCount = count;
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
                await Task.Delay(_updateDelay, ct);
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
