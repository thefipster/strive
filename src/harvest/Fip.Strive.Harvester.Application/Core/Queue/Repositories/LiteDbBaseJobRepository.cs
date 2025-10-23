using Fip.Strive.Harvester.Application.Core.Queue.Models;
using Fip.Strive.Harvester.Application.Infrastructure.Contexts;
using LiteDB;

namespace Fip.Strive.Harvester.Application.Core.Queue.Repositories;

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
