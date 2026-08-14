using System.Threading.Channels;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Application.Features.Jobs.Services;

/// <summary>
/// One pump and N workers. The pump owns the database claim; the workers own execution. Everything
/// runs in this process and nothing is tied to a browser circuit — closing the tab that started a
/// run has no effect on it.
/// </summary>
public sealed class JobRunner(
    IServiceProvider services,
    JobSignal signal,
    IJobNotifier notifier,
    IOptions<JobOptions> options,
    TimeProvider timeProvider,
    ILogger<JobRunner> logger
) : BackgroundService
{
    private readonly JobOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("The job runner is disabled; no jobs will be executed");
            return;
        }

        await RecoverAsync(stoppingToken);

        // Twice the worker count: enough that a worker finishing never waits on the pump, small
        // enough that rows do not sit marked Running in a buffer nobody is working on.
        var capacity = _options.Parallelism * 2;

        var channel = Channel.CreateBounded<Guid>(
            new BoundedChannelOptions(capacity)
            {
                SingleWriter = true,
                FullMode = BoundedChannelFullMode.Wait,
            }
        );

        var workers = Enumerable
            .Range(0, _options.Parallelism)
            .Select(_ => WorkAsync(channel.Reader, stoppingToken))
            .ToList();

        logger.LogInformation(
            "Job runner started with {Parallelism} workers",
            _options.Parallelism
        );

        await PumpAsync(channel.Writer, capacity, stoppingToken);
        channel.Writer.Complete();

        await Task.WhenAll(workers);
    }

    private async Task RecoverAsync(CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IJobStore>();

        if (await store.RecoverInterruptedAsync(cancellationToken) > 0)
            notifier.Notify();
    }

    /// <summary>
    /// Claims only as much as the channel can take. Claiming ahead would flip rows to
    /// <c>Running</c> while they sat in a buffer — a state the jobs page would display and startup
    /// recovery would have to undo.
    /// </summary>
    private async Task PumpAsync(
        ChannelWriter<Guid> writer,
        int capacity,
        CancellationToken stoppingToken
    )
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var claimed = await ClaimAsync(capacity, stoppingToken);

                if (claimed.Count == 0)
                {
                    // Nothing to do: wait for an enqueue, or look again when the poll elapses.
                    await signal.WaitAsync(_options.PollInterval, stoppingToken);
                    continue;
                }

                notifier.Notify();

                foreach (var id in claimed)
                    await writer.WriteAsync(id, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                // A database blip must not kill the pump for the life of the process.
                logger.LogError(exception, "The job pump failed; retrying after the poll interval");

                await Task.Delay(_options.PollInterval, timeProvider, CancellationToken.None);
            }
        }
    }

    private async Task<IReadOnlyList<Guid>> ClaimAsync(int max, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IJobStore>();

        return await store.ClaimAsync(max, cancellationToken);
    }

    private async Task WorkAsync(ChannelReader<Guid> reader, CancellationToken stoppingToken)
    {
        // Read with None: a worker drains what has already been claimed rather than abandoning it,
        // and the per-job token below is what actually stops work in progress.
        await foreach (var id in reader.ReadAllAsync(CancellationToken.None))
            await ExecuteOneAsync(id, stoppingToken);
    }

    /// <summary>
    /// A scope per job, so each gets a short-lived <c>StriveContext</c> rather than one living as
    /// long as the process — and so the handler resolved here gets that scope's dependencies.
    /// </summary>
    private async Task ExecuteOneAsync(Guid id, CancellationToken stoppingToken)
    {
        await using var scope = services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IJobStore>();

        var job = await store.GetAsync(id, CancellationToken.None);

        if (job is null)
        {
            logger.LogWarning("Claimed job {JobId} no longer exists", id);
            return;
        }

        var progress = new ThrottledProgress(
            value => WriteProgressAsync(id, value),
            _options.ProgressInterval,
            timeProvider
        );

        try
        {
            var handler =
                scope
                    .ServiceProvider.GetServices<IJobHandler>()
                    .FirstOrDefault(candidate => candidate.Kind == job.Kind)
                ?? throw new InvalidOperationException(
                    $"No job handler is registered for the kind '{job.Kind}'."
                );

            await handler.ExecuteAsync(new JobContext(job, progress), stoppingToken);

            await progress.FlushAsync();
            await store.CompleteAsync(id, CancellationToken.None);

            logger.LogInformation("Job {JobId} ({Kind}) succeeded", id, job.Kind);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutdown, not failure. Returned to the queue so the next start resumes it, and
            // without spending the single attempt the retry policy allows.
            await store.ReleaseAsync(id, CancellationToken.None);
            logger.LogInformation("Job {JobId} was interrupted by shutdown and re-queued", id);
        }
        catch (Exception exception)
        {
            await progress.FlushAsync();
            await store.FailAsync(id, exception.Message, CancellationToken.None);
        }
        finally
        {
            notifier.Notify();
        }
    }

    /// <summary>
    /// On its own scope, because progress is written from the throttle's fire-and-forget path while
    /// the job's own scope is in use by the handler — and a <c>DbContext</c> is not safe to share
    /// across the two.
    /// </summary>
    private async Task WriteProgressAsync(Guid id, JobProgress value)
    {
        await using var scope = services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IJobStore>();

        await store.SaveProgressAsync(id, value, CancellationToken.None);
        notifier.Notify();
    }
}
