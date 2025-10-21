using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Models;

namespace Fip.Strive.Harvester.Application.Core.Queue.Components.Decorators;

public class ThreadSafeSignalQueue(ISignalQueue component) : ISignalQueue
{
    private readonly SemaphoreSlim _sem = new(1, 1);

    public int Count => component.Count;

    public async Task EnqueueAsync(Signal signal, CancellationToken ct = default)
    {
        await _sem.WaitAsync(ct);
        try
        {
            await component.EnqueueAsync(signal, ct);
        }
        finally
        {
            _sem.Release();
        }
    }

    public async Task<JobEntity?> DequeueAsync(CancellationToken ct = default)
    {
        await _sem.WaitAsync(ct);
        try
        {
            return await component.DequeueAsync(ct);
        }
        finally
        {
            _sem.Release();
        }
    }

    public async Task MarkAsStartedAsync(Guid jobId, CancellationToken ct = default)
    {
        await _sem.WaitAsync(ct);
        try
        {
            await component.MarkAsStartedAsync(jobId, ct);
        }
        finally
        {
            _sem.Release();
        }
    }

    public async Task MarkAsSuccessAsync(Guid jobId, CancellationToken ct = default)
    {
        await _sem.WaitAsync(ct);
        try
        {
            await component.MarkAsSuccessAsync(jobId, ct);
        }
        finally
        {
            _sem.Release();
        }
    }

    public async Task MarkAsFailedAsync(Guid jobId, string reason, CancellationToken ct = default)
    {
        await _sem.WaitAsync(ct);
        try
        {
            await component.MarkAsFailedAsync(jobId, reason, ct);
        }
        finally
        {
            _sem.Release();
        }
    }

    public async Task MarkAsFailedAsync(
        Guid jobId,
        string reason,
        Exception ex,
        CancellationToken ct = default
    )
    {
        await _sem.WaitAsync(ct);
        try
        {
            await component.MarkAsFailedAsync(jobId, reason, ex, ct);
        }
        finally
        {
            _sem.Release();
        }
    }
}
