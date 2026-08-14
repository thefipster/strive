namespace Fip.Strive.Application.Features.Jobs.Services;

/// <summary>
/// Wakes the pump when work is enqueued. A single latching slot rather than a counter: the pump
/// drains whatever it finds, so ten enqueues between two wake-ups need one wake-up, not ten.
/// </summary>
public sealed class JobSignal : IDisposable
{
    private readonly SemaphoreSlim _slot = new(0, 1);

    public void Set()
    {
        try
        {
            if (_slot.CurrentCount == 0)
                _slot.Release();
        }
        catch (SemaphoreFullException)
        {
            // Two callers passed the check together. The slot is set, which is all either wanted;
            // the maximum of one is what turns the race into this exception rather than a counter
            // that climbs and makes the pump spin.
        }
    }

    /// <summary>
    /// Returns when signalled or when <paramref name="timeout"/> elapses. The timeout is the poll
    /// interval, so a lost signal costs latency rather than a stuck queue.
    /// </summary>
    public async Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken) =>
        await _slot.WaitAsync(timeout, cancellationToken);

    public void Dispose() => _slot.Dispose();
}
