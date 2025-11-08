using Fip.Strive.Queue.Storage.Contracts;
using Fip.Strive.Queue.Storage.Lite.Contexts;

namespace Fip.Strive.Queue.Storage.Lite.Repositories;

public class LiteJobDeleter(LiteQueueContext context)
    : LiteDbBaseJobRepository(context),
        IJobDeleter
{
    private readonly LiteQueueContext _context1 = context;

    public int DeleteBefore(DateTime dateTime) =>
        Collection.DeleteMany(x => x.CreatedAt < dateTime);

    public void Rebuild() => _context1.Rebuild();
}
