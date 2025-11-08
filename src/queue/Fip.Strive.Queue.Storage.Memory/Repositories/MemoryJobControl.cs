using Fip.Strive.Queue.Domain.Enums;
using Fip.Strive.Queue.Domain.Models;
using Fip.Strive.Queue.Storage.Contracts;
using Fip.Strive.Queue.Storage.Memory.Contexts;

namespace Fip.Strive.Queue.Storage.Memory.Repositories;

public class MemoryJobControl(MemoryQueueContext context) : IJobControl
{
    public void Insert(JobDetails job) => context.Jobs.Add(job);

    public IEnumerable<JobDetails> GetStored(int count)
    {
        var jobs = context
            .Jobs.Where(x => x.Status == JobStatus.Stored)
            .OrderBy(x => x.CreatedAt)
            .Take(count)
            .ToArray();

        foreach (var job in jobs)
            job.Status = JobStatus.Pending;

        return jobs;
    }

    public void MarkAsStarted(Guid id)
    {
        var job = context.Jobs.FirstOrDefault(x => x.Id == id);
        if (job != null)
        {
            job.Status = JobStatus.Running;
            job.StartedAt = DateTime.UtcNow;
        }
    }

    public void MarkAsSuccess(Guid id)
    {
        var job = context.Jobs.FirstOrDefault(x => x.Id == id);
        if (job != null)
        {
            job.Status = JobStatus.Succeeded;
            job.FinishedAt = DateTime.UtcNow;
        }
    }

    public void MarkAsFailed(Guid id, string message)
    {
        var job = context.Jobs.FirstOrDefault(x => x.Id == id);
        if (job != null)
        {
            job.Status = JobStatus.Failed;
            job.Error = message;
            job.FinishedAt = DateTime.UtcNow;
        }
    }

    public void MarkAsFailed(Guid id, string message, Exception ex)
    {
        var job = context.Jobs.FirstOrDefault(x => x.Id == id);
        if (job != null)
        {
            job.Status = JobStatus.Failed;
            job.Error =
                $"{message}{Environment.NewLine}{ex.GetType().Name}: {ex.Message}{Environment.NewLine}{ex.StackTrace}";
            job.FinishedAt = DateTime.UtcNow;
        }
    }

    public void Reset()
    {
        var jobs = context.Jobs.Where(x => x.Status == JobStatus.Pending).ToArray();

        foreach (var job in jobs)
            job.Status = JobStatus.Stored;
    }

    public void Dispose()
    {
        context.Jobs.Clear();
    }
}
