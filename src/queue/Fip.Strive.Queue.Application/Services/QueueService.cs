using System.Collections.Concurrent;
using Fip.Strive.Queue.Application.Services.Contracts;
using Fip.Strive.Queue.Domain.Enums;
using Fip.Strive.Queue.Domain.Models;
using Fip.Strive.Queue.Storage.Contracts;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Queue.Application.Services;

public class QueueService : IQueueService, IDisposable
{
    private readonly IJobControl _jobs;
    private readonly IOptions<QueueConfig> _config;
    private readonly ConcurrentQueue<JobDetails> _queue = new();
    private readonly QueueMetrics _metrics;

    private bool QueueShouldBeRefilled =>
        _queue.Count < _config.Value.QueueCountLimit - _config.Value.QueueBatchSize;

    public bool IsReady { get; private set; }

    public QueueService(IJobControl jobs, IOptions<QueueConfig> config, QueueMetrics metrics)
    {
        _config = config;
        _metrics = metrics;

        _jobs = jobs;
        _jobs.Reset();

        IsReady = true;
    }

    public int Count => _queue.Count;

    public Task EnqueueAsync(Signal signal, CancellationToken ct = default)
    {
        var job = signal.ToJobEntity();
        _jobs.Insert(job);

        if (_config.Value.QueueCountLimit > _queue.Count)
        {
            job.Status = JobStatus.Pending;
            Enqueue(job);
        }

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
            Enqueue(job);
    }

    private void Enqueue(JobDetails job)
    {
        _queue.Enqueue(job);
        _metrics.RecordEnqueue();
    }
}
