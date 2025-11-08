using Fip.Strive.Queue.Domain.Enums;
using Fip.Strive.Queue.Domain.Models;
using Fip.Strive.Queue.Storage.Contracts;
using Fip.Strive.Queue.Storage.Postgres.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Queue.Storage.Postgres.Repositories;

public class PostgresJobControl(PostgresQueueContext context) : IJobControl
{
    public void Insert(JobDetails job)
    {
        context.Jobs.Add(job);
        context.SaveChanges();
    }

    public IEnumerable<JobDetails> GetStored(int count) =>
        context
            .Jobs.AsNoTracking()
            .Where(x => x.Status == JobStatus.Pending || x.Status == JobStatus.Stored)
            .OrderBy(x => x.CreatedAt)
            .Take(count)
            .ToArray();

    public void MarkAsStarted(Guid id)
    {
        context
            .Jobs.Where(j => j.Id == id)
            .ExecuteUpdate(s =>
                s.SetProperty(j => j.Status, JobStatus.Running)
                    .SetProperty(j => j.StartedAt, DateTime.UtcNow)
            );
    }

    public void MarkAsSuccess(Guid id)
    {
        context
            .Jobs.Where(j => j.Id == id)
            .ExecuteUpdate(s =>
                s.SetProperty(j => j.Status, JobStatus.Succeeded)
                    .SetProperty(j => j.FinishedAt, DateTime.UtcNow)
            );
    }

    public void MarkAsFailed(Guid id, string message)
    {
        context
            .Jobs.Where(j => j.Id == id)
            .ExecuteUpdate(s =>
                s.SetProperty(j => j.Status, JobStatus.Failed)
                    .SetProperty(j => j.FinishedAt, DateTime.UtcNow)
                    .SetProperty(j => j.Error, message)
            );
    }

    public void MarkAsFailed(Guid id, string message, Exception exception)
    {
        context
            .Jobs.Where(j => j.Id == id)
            .ExecuteUpdate(s =>
                s.SetProperty(j => j.Status, JobStatus.Failed)
                    .SetProperty(j => j.FinishedAt, DateTime.UtcNow)
                    .SetProperty(
                        j => j.Error,
                        $"{message}{Environment.NewLine}{exception.GetType().Name}: {exception.Message}{Environment.NewLine}{Environment.NewLine}{exception.StackTrace}"
                    )
            );
    }

    public void Reset()
    {
        context
            .Jobs.Where(j => j.Status == JobStatus.Pending)
            .ExecuteUpdate(s => s.SetProperty(j => j.Status, JobStatus.Stored));
    }
}
