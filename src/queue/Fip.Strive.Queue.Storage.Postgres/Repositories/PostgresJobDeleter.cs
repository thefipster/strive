using Fip.Strive.Queue.Storage.Contracts;
using Fip.Strive.Queue.Storage.Postgres.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Queue.Storage.Postgres.Repositories;

public class PostgresJobDeleter(PostgresQueueContext context) : IJobDeleter
{
    public int DeleteBefore(DateTime maxDate)
    {
        return context.Jobs.Where(j => j.FinishedAt < maxDate).ExecuteDelete();
    }
}
