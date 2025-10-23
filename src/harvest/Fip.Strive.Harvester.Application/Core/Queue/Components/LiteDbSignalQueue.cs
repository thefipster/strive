using System.Collections.Concurrent;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Enums;
using Fip.Strive.Harvester.Application.Core.Queue.Models;
using Fip.Strive.Harvester.Application.Core.Queue.Repositories.Contracts;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Core.Queue.Components;

public class LiteDbSignalQueue(IJobControl jobs, IOptions<QueueConfig> config)
    : ISignalQueue,
        IDisposable
{
    private readonly ConcurrentQueue<JobDetails> _queue = new();

    private bool QueueShouldBeRefilled =>
        _queue.Count < config.Value.QueueCountLimit - config.Value.QueueBatchSize;

    public int Count => _queue.Count;

    public Task EnqueueAsync(Signal signal, CancellationToken ct = default)
    {
        var job = signal.ToJobEntity();

        if (config.Value.QueueCountLimit < _queue.Count)
        {
            job.Status = JobStatus.Pending;
            _queue.Enqueue(job);
        }

        jobs.Insert(job);

        return Task.CompletedTask;
    }

    public async Task<JobDetails?> DequeueAsync(CancellationToken ct = default)
    {
        var job = await TryDequeueStartedJobAsync();

        if (QueueShouldBeRefilled)
            RefillQueue();

        return job;
    }

    public Task MarkAsStartedAsync(Guid jobId, CancellationToken ct = default)
    {
        jobs.MarkAsStarted(jobId);
        return Task.CompletedTask;
    }

    public Task MarkAsSuccessAsync(Guid jobId, CancellationToken ct = default)
    {
        jobs.MarkAsSuccess(jobId);
        return Task.CompletedTask;
    }

    public Task MarkAsFailedAsync(Guid jobId, string reason, CancellationToken ct = default)
    {
        jobs.MarkAsFailed(jobId, reason);
        return Task.CompletedTask;
    }

    public Task MarkAsFailedAsync(
        Guid jobId,
        string reason,
        Exception ex,
        CancellationToken ct = default
    )
    {
        jobs.MarkAsFailed(jobId, reason, ex);
        return Task.CompletedTask;
    }

    public void Dispose() => jobs.Dispose();

    private Task<JobDetails?> TryDequeueStartedJobAsync()
    {
        if (_queue.TryDequeue(out var job))
            jobs.MarkAsStarted(job.Id);

        return Task.FromResult(job);
    }

    private void RefillQueue()
    {
        var storedJobs = jobs.GetStored(config.Value.QueueBatchSize);

        foreach (var job in storedJobs)
            _queue.Enqueue(job);
    }
}
