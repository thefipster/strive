namespace Fip.Strive.Application.Features.Jobs.Services;

/// <summary>
/// Decides when a view should re-read after a burst of change notifications. A running job notifies
/// far more often than a screen can usefully change, but the <em>last</em> notification is the one
/// that carries the finished state — so this coalesces rather than drops.
/// </summary>
/// <remarks>
/// A leading-edge-only throttle is the obvious implementation and is wrong here: it lets the first
/// notification of a burst through and discards the rest, including the terminal one, leaving a
/// completed job displayed as still running until something unrelated happens to notify again.
/// </remarks>
public sealed class RefreshWindow(TimeSpan interval)
{
    private readonly Lock _gate = new();

    private DateTimeOffset _last = DateTimeOffset.MinValue;
    private bool _scheduled;

    /// <summary>
    /// How long to wait before re-reading, or <c>null</c> when a refresh is already scheduled and
    /// this notification is folded into it.
    /// </summary>
    public TimeSpan? Request(DateTimeOffset now)
    {
        lock (_gate)
        {
            if (_scheduled)
                return null;

            _scheduled = true;

            var elapsed = now - _last;
            return elapsed >= interval ? TimeSpan.Zero : interval - elapsed;
        }
    }

    /// <summary>Called once the scheduled refresh has run, opening the window for the next one.</summary>
    public void Completed(DateTimeOffset now)
    {
        lock (_gate)
        {
            _scheduled = false;
            _last = now;
        }
    }
}
