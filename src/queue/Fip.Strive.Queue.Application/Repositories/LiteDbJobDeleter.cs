using Fip.Strive.Queue.Application.Contexts;
using Fip.Strive.Queue.Application.Repositories.Contracts;

namespace Fip.Strive.Queue.Application.Repositories;

public class LiteDbJobDeleter(SignalQueueContext context)
    : LiteDbBaseJobRepository(context),
        IJobDeleter
{
    private readonly SignalQueueContext _context1 = context;

    public int DeleteBefore(DateTime dateTime) =>
        Collection.DeleteMany(x => x.CreatedAt < dateTime);

    public void Rebuild() => _context1.Rebuild();
}
