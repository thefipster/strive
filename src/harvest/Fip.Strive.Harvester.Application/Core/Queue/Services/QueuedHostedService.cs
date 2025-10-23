using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Core.Queue.Services;

public class QueuedHostedService(
    IOptions<QueueConfig> config,
    ILogger<QueuedHostedService> logger,
    IQueueWorkerFactory workerFactory
) : BackgroundService
{
    private readonly object _lock = new();
    private CancellationTokenSource? _runCts;
    private Task? _runTask;
    private readonly QueueMetrics _metrics = new(config);

    public bool IsRunning => _runTask is { IsCompleted: false };

    public void StartWork()
    {
        lock (_lock)
        {
            if (IsRunning)
            {
                logger.LogWarning("QueuedHostedService is already running.");
                return;
            }

            logger.LogInformation("Starting QueuedHostedService workers.");
            _runCts = new CancellationTokenSource();
            _runTask = RunInternalAsync(_runCts.Token);
        }
    }

    public async Task StopWorkAsync()
    {
        lock (_lock)
        {
            if (!IsRunning)
                return;

            logger.LogInformation("Stopping QueuedHostedService workers.");
            _runCts?.Cancel();
        }

        if (_runTask is not null)
        {
            try
            {
                await _runTask;
            }
            catch (OperationCanceledException) { }
        }

        _runTask = null;
        _runCts = null;
    }

    private async Task RunInternalAsync(CancellationToken ct)
    {
        var workers = Enumerable
            .Range(0, config.Value.MaxDegreeOfParallelism)
            .Select(id => Task.Run(() => workerFactory.CreateRunner(_metrics).RunAsync(id, ct), ct))
            .ToList();

        workers.Add(Task.Run(() => workerFactory.CreateReporter(_metrics).RunAsync(ct), ct));

        await Task.WhenAll(workers);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("QueuedHostedService initialized.");

        StartWork();
        // Keep running until the host shuts down
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
