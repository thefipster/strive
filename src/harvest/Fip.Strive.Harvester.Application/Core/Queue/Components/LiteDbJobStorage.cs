using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Enums;
using Fip.Strive.Harvester.Application.Core.Queue.Models;
using Fip.Strive.Harvester.Application.Infrastructure.Contexts;
using LiteDB;

namespace Fip.Strive.Harvester.Application.Core.Queue.Components;

public class LiteDbJobStorage : IJobStorage, IDisposable
{
    private readonly ILiteCollection<JobEntity> _collection;
    private readonly SignalQueueContext _context;

    public LiteDbJobStorage(SignalQueueContext context)
    {
        _context = context;

        _collection = context.GetCollection<JobEntity>();
        _collection.EnsureIndex(x => x.Status);
        _collection.EnsureIndex(x => x.CreatedAt);
        _collection.EnsureIndex(x => x.Type);
    }

    public void Insert(JobEntity job) => _collection.Insert(job);

    public IEnumerable<JobEntity> GetPending(int count) =>
        _collection.Find(x => x.Status == JobStatus.Pending).OrderBy(x => x.CreatedAt).Take(count);

    public IEnumerable<JobEntity> GetCompleted(int count) =>
        _collection
            .Find(x => x.Status == JobStatus.Failed || x.Status == JobStatus.Succeeded)
            .OrderBy(x => x.FinishedAt)
            .Take(count);

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

    public void MarkAsFailed(Guid id, string message, Exception exception)
    {
        var job = _collection.FindOne(x => x.Id == id);
        job.FinishedAt = DateTime.UtcNow;
        job.Status = JobStatus.Failed;
        job.Error =
            $"{message}{Environment.NewLine}{exception.GetType().Name}: {exception.Message}{Environment.NewLine}{Environment.NewLine}{exception.StackTrace}";
        _collection.Update(job);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
