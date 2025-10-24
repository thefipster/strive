using System.Collections.Concurrent;
using Fip.Strive.Core.Domain.Schemas.Queue.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Models;
using Fip.Strive.Harvester.Application.Core.Queue.Repositories.Contracts;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Core.Queue.Components;

public class LiteDbSignalQueue : ISignalQueue, IDisposable
{
    private readonly IJobControl _jobs;
    private readonly IOptions<QueueConfig> _config;
    private readonly ConcurrentQueue<JobDetails> _queue = new();

    private bool QueueShouldBeRefilled =>
        _queue.Count < _config.Value.QueueCountLimit - _config.Value.QueueBatchSize;

    public LiteDbSignalQueue(IJobControl jobs, IOptions<QueueConfig> config)
    {
        _config = config;

        _jobs = jobs;
        _jobs.Reset();
    }

    public int Count => _queue.Count;

    public Task EnqueueAsync(Signal signal, CancellationToken ct = default)
    {
        var job = signal.ToJobEntity();

        if (_config.Value.QueueCountLimit > _queue.Count)
        {
            job.Status = JobStatus.Pending;
            _queue.Enqueue(job);
        }

        _jobs.Insert(job);

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
        _jobs.MarkAsStarted(jobId);
        return Task.CompletedTask;
    }

    public Task MarkAsSuccessAsync(Guid jobId, CancellationToken ct = default)
    {
        _jobs.MarkAsSuccess(jobId);
        return Task.CompletedTask;
    }

    public Task MarkAsFailedAsync(Guid jobId, string reason, CancellationToken ct = default)
    {
        _jobs.MarkAsFailed(jobId, reason);
        return Task.CompletedTask;
    }

    public Task MarkAsFailedAsync(
        Guid jobId,
        string reason,
        Exception ex,
        CancellationToken ct = default
    )
    {
        _jobs.MarkAsFailed(jobId, reason, ex);
        return Task.CompletedTask;
    }

    public void Dispose() => _jobs.Dispose();

    private Task<JobDetails?> TryDequeueStartedJobAsync()
    {
        if (_queue.TryDequeue(out var job))
            _jobs.MarkAsStarted(job.Id);

        return Task.FromResult(job);
    }

    private void RefillQueue()
    {
        var storedJobs = _jobs.GetStored(_config.Value.QueueBatchSize);

        foreach (var job in storedJobs)
            _queue.Enqueue(job);
    }
}
