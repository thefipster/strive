using Fip.Strive.Queue.Storage.Contracts;
using Fip.Strive.Queue.Storage.Memory.Contexts;

namespace Fip.Strive.Queue.Storage.Memory.Repositories;

public class MemoryJobDeleter(MemoryQueueContext context) : IJobDeleter
{
    public int DeleteBefore(DateTime dateTime) =>
        context.Jobs.RemoveWhere(x => x.FinishedAt < dateTime);
}
