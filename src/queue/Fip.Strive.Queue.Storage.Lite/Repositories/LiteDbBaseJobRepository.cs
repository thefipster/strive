using Fip.Strive.Queue.Domain.Models;
using Fip.Strive.Queue.Storage.Lite.Contexts;
using LiteDB;

namespace Fip.Strive.Queue.Storage.Lite.Repositories;

public class LiteDbBaseJobRepository
{
    protected readonly ILiteCollection<JobDetails> Collection;
    private readonly LiteQueueContext _context;

    public LiteDbBaseJobRepository(LiteQueueContext context)
    {
        _context = context;

        Collection = context.GetCollection<JobDetails>();
        Collection.EnsureIndex(x => x.Status);
        Collection.EnsureIndex(x => x.CreatedAt);
        Collection.EnsureIndex(x => x.Type);
    }
}
