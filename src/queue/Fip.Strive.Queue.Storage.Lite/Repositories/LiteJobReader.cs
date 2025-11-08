using Fip.Strive.Core.Domain.Schemas.Requests.Paging;
using Fip.Strive.Queue.Domain.Enums;
using Fip.Strive.Queue.Domain.Models;
using Fip.Strive.Queue.Storage.Contracts;
using Fip.Strive.Queue.Storage.Lite.Contexts;

namespace Fip.Strive.Queue.Storage.Lite.Repositories;

public class LiteJobReader(LiteQueueContext context) : LiteDbBaseJobRepository(context), IJobReader
{
    public PagedResponse<JobDetails> GetUpcommingJobs(int page, int size)
    {
        var query = Collection
            .Query()
            .Where(x => x.Status == JobStatus.Stored || x.Status == JobStatus.Pending)
            .OrderByDescending(x => x.CreatedAt);

        var count = query.Count();
        var items = query.Skip(page * size).Limit(size).ToArray();

        return new PagedResponse<JobDetails>(items, count);
    }

    public PagedResponse<JobDetails> GetSucceededJobs(int page, int size) =>
        GetJobs(JobStatus.Succeeded, page, size);

    public PagedResponse<JobDetails> GetFailedJobs(int page, int size) =>
        GetJobs(JobStatus.Failed, page, size);

    public PagedResponse<JobDetails> GetJobs(JobStatus status, int page, int size)
    {
        var query = Collection
            .Query()
            .Where(x => x.Status == status)
            .OrderByDescending(x => x.CreatedAt);

        var count = query.Count();
        var items = query.Skip(page * size).Limit(size).ToArray();

        return new PagedResponse<JobDetails>(items, count);
    }

    public PagedResponse<JobDetails> GetJobs(int page, int size, params JobStatus[] statuses)
    {
        var query = Collection.Query();

        query = query.Where(x => statuses.Contains(x.Status));
        query = query.OrderByDescending(x => x.CreatedAt);

        var count = query.Count();
        var items = query.Skip(page * size).Limit(size).ToArray();

        return new PagedResponse<JobDetails>(items, count);
    }
}
