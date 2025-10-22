using Fip.Strive.Harvester.Application.Core.Queue.Enums;
using Fip.Strive.Harvester.Application.Core.Queue.Models;
using Fip.Strive.Harvester.Application.Core.Queue.Repositories.Contracts;
using Fip.Strive.Harvester.Application.Infrastructure.Contexts;

namespace Fip.Strive.Harvester.Application.Core.Queue.Repositories;

public class LiteDbJobStorage(SignalQueueContext context)
    : LiteDbBaseJobRepository(context),
        IJobStorage
{
    public void Insert(JobDetails job) => Collection.Insert(job);

    public IEnumerable<JobDetails> GetStored(int count)
    {
        var jobs = Collection
            .Find(x => x.Status == JobStatus.Stored)
            .OrderBy(x => x.CreatedAt)
            .Take(count)
            .ToList();

        if (!jobs.Any())
            return Enumerable.Empty<JobDetails>();

        var ids = jobs.Select(x => x.Id).ToArray();
        Collection.UpdateMany(
            x => new JobDetails { Status = JobStatus.Pending },
            x => ids.Contains(x.Id)
        );

        return jobs;
    }

    public IEnumerable<JobDetails> GetCompleted(int count) =>
        Collection
            .Find(x => x.Status == JobStatus.Failed || x.Status == JobStatus.Succeeded)
            .OrderBy(x => x.FinishedAt)
            .Take(count);

    public void MarkAsStarted(Guid id)
    {
        var job = Collection.FindOne(x => x.Id == id);
        job.StartedAt = DateTime.UtcNow;
        Collection.Update(job);
    }

    public void MarkAsSuccess(Guid id)
    {
        var job = Collection.FindOne(x => x.Id == id);
        job.FinishedAt = DateTime.UtcNow;
        job.Status = JobStatus.Succeeded;
        Collection.Update(job);
    }

    public void MarkAsFailed(Guid id, string message)
    {
        var job = Collection.FindOne(x => x.Id == id);
        job.FinishedAt = DateTime.UtcNow;
        job.Status = JobStatus.Failed;
        job.Error = message;
        Collection.Update(job);
    }

    public void MarkAsFailed(Guid id, string message, Exception exception) =>
        MarkAsFailed(
            id,
            $"{message}{Environment.NewLine}{exception.GetType().Name}: {exception.Message}{Environment.NewLine}{Environment.NewLine}{exception.StackTrace}"
        );
}
