using Fip.Strive.Core.Domain.Schemas.Queue.Models;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Core.Queue.Components;

public class QueueRunner(
    ISignalQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<QueueRunner> logger,
    QueueMetrics metrics,
    IOptions<QueueConfig> config
) : IQueueRunner
{
    private readonly TimeSpan _processingDelay = TimeSpan.FromMilliseconds(
        config.Value.ProcessingDelayMs
    );

    private bool _isRunning = false;

    public bool IsRunning => _isRunning;

    public async Task RunAsync(int workerId, CancellationToken ct)
    {
        logger.LogInformation("QueueRunner {WorkerId} starting.", workerId);
        _isRunning = true;

        while (!ct.IsCancellationRequested)
            await LoopAsync(workerId, ct);

        logger.LogInformation("QueueRunner {WorkerId} stopping.", workerId);
        _isRunning = false;
    }

    private async Task LoopAsync(int workerId, CancellationToken ct)
    {
        var job = await queue.DequeueAsync(ct);
        if (job != null)
        {
            try
            {
                await RunJobAsync(ct, job);
            }
            catch (OperationCanceledException)
            {
                await queue.MarkAsFailedAsync(job.Id, "Operation cancelled.", ct);
                logger.LogInformation("Operation cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                await queue.MarkAsFailedAsync(job.Id, $"Worker {workerId} error", ex, ct);
                logger.LogError(ex, "Error in worker {WorkerId}.", workerId);
            }
            finally
            {
                metrics.DecrementActiveWorkers();
            }
        }

        await Task.Delay(_processingDelay, ct);
    }

    private async Task RunJobAsync(CancellationToken ct, JobDetails job)
    {
        metrics.IncrementActiveWorkers();
        await queue.MarkAsStartedAsync(job.Id, ct);

        await ExecuteWorkerAsync(job, ct);

        await queue.MarkAsSuccessAsync(job.Id, ct);
        metrics.RecordCompletion();
    }

    private async Task ExecuteWorkerAsync(JobDetails job, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();

        var worker = scope
            .ServiceProvider.GetRequiredService<IEnumerable<ISignalQueueWorker>>()
            .FirstOrDefault(w => w.Type == job.Type);

        if (worker == null)
            throw new InvalidOperationException($"No worker found for type {job.Type}");

        await worker.ProcessAsync(job, ct);
    }
}
