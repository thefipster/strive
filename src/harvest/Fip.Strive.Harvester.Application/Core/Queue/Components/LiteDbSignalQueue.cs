using System.Collections.Concurrent;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Models;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Core.Queue.Services;

public class LiteDbSignalQueue(IJobStorage jobs, IOptions<QueueConfig> config)
    : ISignalQueue,
        IDisposable
{
    private readonly Lock _lock = new();
    private readonly ConcurrentQueue<JobEntity> _queue = new();

    private bool QueueIsBelowThreshold =>
        _queue.Count < config.Value.QueueCountLimit - config.Value.QueueBatchSize;

    public int Count => _queue.Count;

    public Task EnqueueAsync(Signal signal, CancellationToken ct = default)
    {
        var entity = signal.ToJobEntity();

        lock (_lock)
        {
            jobs.Insert(entity);

            if (config.Value.QueueCountLimit < _queue.Count)
                _queue.Enqueue(entity);
        }

        return Task.CompletedTask;
    }

    public Task<JobEntity?> DequeueAsync(CancellationToken ct = default)
    {
        lock (_lock)
            return TryDequeueAndRefill();
    }

    public Task MarkAsSuccessAsync(Guid jobId)
    {
        lock (_lock)
            jobs.MarkAsSuccess(jobId);

        return Task.CompletedTask;
    }

    public Task MarkAsFailedAsync(Guid jobId, Exception ex)
    {
        lock (_lock)
            jobs.MarkAsFailed(jobId, ex);

        return Task.CompletedTask;
    }

    public void Dispose() => jobs.Dispose();

    private Task<JobEntity?> TryDequeueAndRefill()
    {
        var job = TryDequeueStartedJob();

        if (QueueIsBelowThreshold)
            RefillQueue();

        return job;
    }

    private Task<JobEntity?> TryDequeueStartedJob()
    {
        if (_queue.TryDequeue(out var job))
            jobs.MarkAsStarted(job.Id);

        return Task.FromResult(job);
    }

    private void RefillQueue()
    {
        var storedJobs = jobs.GetPending(config.Value.QueueBatchSize);

        foreach (var job in storedJobs)
            _queue.Enqueue(job);
    }
}
