using Fip.Strive.Queue.Domain.Enums;
using Fip.Strive.Queue.Domain.Models;
using Fip.Strive.Queue.Storage.Contracts;
using Fip.Strive.Queue.Storage.Lite.Contexts;
using LiteDB;

namespace Fip.Strive.Queue.Storage.Lite.Repositories;

public class LiteDbJobControl : IJobControl
{
    private readonly SignalQueueContext _context;
    private readonly ILiteCollection<JobDetails> _collection;
    private bool _disposed;

    public LiteDbJobControl(SignalQueueContext context)
    {
        _context = context;

        _collection = context.GetCollection<JobDetails>();
        _collection.EnsureIndex(x => x.Status);
        _collection.EnsureIndex(x => x.CreatedAt);
        _collection.EnsureIndex(x => x.Type);
    }

    public void Insert(JobDetails job) => _collection.Insert(job);

    public IEnumerable<JobDetails> GetStored(int count)
    {
        var jobs = _collection
            .Find(x => x.Status == JobStatus.Stored)
            .OrderBy(x => x.CreatedAt)
            .Take(count)
            .ToList();

        if (!jobs.Any())
            return Enumerable.Empty<JobDetails>();

        var ids = jobs.Select(x => x.Id).ToArray();
        _collection.UpdateMany(
            x => new JobDetails { Status = JobStatus.Pending },
            x => ids.Contains(x.Id)
        );

        return jobs;
    }

    public void MarkAsStarted(Guid id)
    {
        var job = _collection.FindOne(x => x.Id == id);
        job.StartedAt = DateTime.UtcNow;
        _collection.Update(job);
    }

    public void MarkAsSuccess(Guid id)
    {
        var job = _collection.FindOne(x => x.Id == id);
        job.FinishedAt = DateTime.UtcNow;
        job.Status = JobStatus.Succeeded;
        _collection.Update(job);
    }

    public void MarkAsFailed(Guid id, string message)
    {
        var job = _collection.FindOne(x => x.Id == id);
        job.FinishedAt = DateTime.UtcNow;
        job.Status = JobStatus.Failed;
        job.Error = message;
        _collection.Update(job);
    }

    public void MarkAsFailed(Guid id, string message, Exception exception) =>
        MarkAsFailed(
            id,
            $"{message}{Environment.NewLine}{exception.GetType().Name}: {exception.Message}{Environment.NewLine}{Environment.NewLine}{exception.StackTrace}"
        );

    public void Reset()
    {
        var ids = _collection
            .Find(x => x.Status == JobStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .Select(x => x.Id)
            .ToArray();

        _collection.UpdateMany(
            x => new JobDetails { Status = JobStatus.Stored },
            x => ids.Contains(x.Id)
        );
    }

    protected void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
