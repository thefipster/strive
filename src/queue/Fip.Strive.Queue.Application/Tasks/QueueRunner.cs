using Fip.Strive.Queue.Application.Services.Contracts;
using Fip.Strive.Queue.Application.Tasks.Contracts;
using Fip.Strive.Queue.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Queue.Application.Tasks;

public class QueueRunner(
    IQueueService queue,
    IServiceScopeFactory scopeFactory,
    ILogger<QueueRunner> logger,
    QueueMetrics metrics,
    IOptions<QueueConfig> config
) : IQueueRunner
{
    private readonly TimeSpan _processingDelay = TimeSpan.FromMilliseconds(
        config.Value.ProcessingDelayMs
    );

    private bool _isRunning;

    public bool IsRunning => _isRunning;

    public Guid Id { get; } = Guid.NewGuid();

    public async Task RunAsync(CancellationToken ct)
    {
        logger.LogInformation("QueueRunner {WorkerId} starting.", Id);
        _isRunning = true;

        while (!ct.IsCancellationRequested)
            await LoopAsync(Id, ct);

        logger.LogInformation("QueueRunner {WorkerId} stopping.", Id);
        _isRunning = false;
    }

    private async Task LoopAsync(Guid workerId, CancellationToken ct)
    {
        var job = await queue.DequeueAsync(ct);
        if (job != null)
        {
            try
            {
                metrics.IncrementActiveWorkers();
                logger.LogInformation(
                    "Running job {JobId} in worker {WorkerId}.",
                    job.Id,
                    workerId
                );
                await RunJobAsync(ct, job);
            }
            catch (OperationCanceledException)
            {
                await queue.MarkAsFailedAsync(job.Id, "Operation cancelled.", ct);
                logger.LogInformation("Operation cancelled in worker {WorkerId}.", workerId);
                throw;
            }
            catch (Exception ex)
            {
                await queue.MarkAsFailedAsync(job.Id, $"Worker {workerId} error", ex, ct);
                logger.LogError(ex, "Error in worker {WorkerId}.", workerId);
            }
            finally
            {
                metrics.RecordCompletion();
                metrics.DecrementActiveWorkers();
            }
        }

        await Task.Delay(_processingDelay, ct);
    }

    private async Task RunJobAsync(CancellationToken ct, JobDetails job)
    {
        await queue.MarkAsStartedAsync(job.Id, ct);

        await ExecuteWorkerAsync(job, ct);

        await queue.MarkAsSuccessAsync(job.Id, ct);
    }

    private async Task ExecuteWorkerAsync(JobDetails job, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();

        var worker = scope
            .ServiceProvider.GetRequiredService<IEnumerable<IQueueWorker>>()
            .FirstOrDefault(w => w.Type == job.Type);

        if (worker == null)
            throw new InvalidOperationException($"No worker found for type {job.Type}");

        await worker.ProcessAsync(job, ct);
    }
}
