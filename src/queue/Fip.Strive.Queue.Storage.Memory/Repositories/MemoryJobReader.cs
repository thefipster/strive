using Fip.Strive.Core.Domain.Schemas.Requests.Paging;
using Fip.Strive.Queue.Domain.Enums;
using Fip.Strive.Queue.Domain.Models;
using Fip.Strive.Queue.Storage.Contracts;
using Fip.Strive.Queue.Storage.Memory.Contexts;

namespace Fip.Strive.Queue.Storage.Memory.Repositories;

public class MemoryJobReader(MemoryQueueContext context) : IJobReader
{
    public PagedResponse<JobDetails> GetJobs(int page, int size, params JobStatus[] statuses)
    {
        var count = context
            .Jobs.Where(x => statuses.Contains(x.Status))
            .OrderByDescending(x => x.CreatedAt)
            .Count();

        var items = context
            .Jobs.Where(x => statuses.Contains(x.Status))
            .OrderByDescending(x => x.CreatedAt)
            .Skip(page * size)
            .Take(size)
            .ToArray();

        return new PagedResponse<JobDetails>(items, count);
    }
}
