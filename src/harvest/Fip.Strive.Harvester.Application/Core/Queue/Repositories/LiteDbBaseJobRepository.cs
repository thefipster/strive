using Fip.Strive.Harvester.Application.Core.Queue.Models;
using Fip.Strive.Harvester.Application.Infrastructure.Contexts;
using LiteDB;

namespace Fip.Strive.Harvester.Application.Core.Queue.Repositories;

public class LiteDbBaseJobRepository : IDisposable
{
    protected readonly ILiteCollection<JobDetails> Collection;
    private readonly SignalQueueContext _context;
    private bool _disposed;

    public LiteDbBaseJobRepository(SignalQueueContext context)
    {
        _context = context;

        Collection = context.GetCollection<JobDetails>();
        Collection.EnsureIndex(x => x.Status);
        Collection.EnsureIndex(x => x.CreatedAt);
        Collection.EnsureIndex(x => x.Type);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
