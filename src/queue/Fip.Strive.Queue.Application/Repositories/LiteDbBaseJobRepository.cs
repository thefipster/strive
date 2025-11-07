using Fip.Strive.Queue.Application.Contexts;
using Fip.Strive.Queue.Domain.Models;
using LiteDB;

namespace Fip.Strive.Queue.Application.Repositories;

public class LiteDbBaseJobRepository
{
    protected readonly ILiteCollection<JobDetails> Collection;
    private readonly SignalQueueContext _context;

    public LiteDbBaseJobRepository(SignalQueueContext context)
    {
        _context = context;

        Collection = context.GetCollection<JobDetails>();
        Collection.EnsureIndex(x => x.Status);
        Collection.EnsureIndex(x => x.CreatedAt);
        Collection.EnsureIndex(x => x.Type);
    }
}
