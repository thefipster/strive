using Fip.Strive.Harvester.Application.Core.Queue.Repositories.Contracts;
using Fip.Strive.Harvester.Application.Infrastructure.Contexts;

namespace Fip.Strive.Harvester.Application.Core.Queue.Repositories;

public class LiteDbJobDeleter(SignalQueueContext context)
    : LiteDbBaseJobRepository(context),
        IJobDeleter
{
    private readonly SignalQueueContext _context1 = context;

    public int DeleteBefore(DateTime dateTime) =>
        Collection.DeleteMany(x => x.CreatedAt < dateTime);

    public void Rebuild() => _context1.Rebuild();
}
