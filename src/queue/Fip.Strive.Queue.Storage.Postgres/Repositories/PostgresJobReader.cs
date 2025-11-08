using Fip.Strive.Core.Domain.Schemas.Requests.Paging;
using Fip.Strive.Queue.Domain.Enums;
using Fip.Strive.Queue.Domain.Models;
using Fip.Strive.Queue.Storage.Contracts;
using Fip.Strive.Queue.Storage.Postgres.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Queue.Storage.Postgres.Repositories;

public class PostgresJobReader(PostgresQueueContext context) : IJobReader
{
    public PagedResponse<JobDetails> GetJobs(int page, int size, params JobStatus[] statuses)
    {
        var query = context
            .Jobs.AsNoTracking()
            .Where(x => statuses.Contains(x.Status))
            .OrderByDescending(x => x.CreatedAt);

        var count = query.Count();
        var items = query.Skip(page * size).Take(size).ToArray();

        return new PagedResponse<JobDetails>(items, count);
    }
}
