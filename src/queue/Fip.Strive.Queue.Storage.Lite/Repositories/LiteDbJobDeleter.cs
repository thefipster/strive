using Fip.Strive.Queue.Storage.Contracts;
using Fip.Strive.Queue.Storage.Lite.Contexts;

namespace Fip.Strive.Queue.Storage.Lite.Repositories;

public class LiteDbJobDeleter(SignalQueueContext context)
    : LiteDbBaseJobRepository(context),
        IJobDeleter
{
    private readonly SignalQueueContext _context1 = context;

    public int DeleteBefore(DateTime dateTime) =>
        Collection.DeleteMany(x => x.CreatedAt < dateTime);

    public void Rebuild() => _context1.Rebuild();
}
