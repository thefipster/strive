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
    private bool _changedSinceRequest;

    /// <summary>
    /// How long to wait before re-reading, or <c>null</c> when a refresh is already scheduled and
    /// this notification is folded into it.
    /// </summary>
    public TimeSpan? Request(DateTimeOffset now)
    {
        lock (_gate)
        {
            if (_scheduled)
            {
                // Folded, but remembered. A notification only arrives once its write has landed, so
                // if the scheduled refresh has already read by now this is a change it cannot have
                // seen — and folding it away silently is what left a finished job on screen as
                // running.
                _changedSinceRequest = true;
                return null;
            }

            _scheduled = true;
            _changedSinceRequest = false;

            return DelayFrom(now);
        }
    }

    /// <summary>
    /// Called once the scheduled refresh has run. Returns how long to wait before reading again
    /// when something was announced while it was in flight, or <c>null</c> when the burst is over
    /// and the read that just happened is the last word.
    /// </summary>
    /// <remarks>
    /// The follow-up matters most for the very last notification of a run: the job it announces has
    /// finished, so nothing is left to notify again and re-read on the page's behalf. Without it the
    /// row keeps whatever the in-flight read caught — for a job whose final progress write and
    /// terminal write are a round-trip apart, that is a full progress bar on a row still marked
    /// running.
    /// </remarks>
    public TimeSpan? Completed(DateTimeOffset now)
    {
        lock (_gate)
        {
            _last = now;

            if (!_changedSinceRequest)
            {
                _scheduled = false;
                return null;
            }

            // Held closed rather than reopened: the caller reads again on the strength of this
            // return, and a notification arriving before it does must fold into that read instead
            // of starting a second one alongside it.
            _changedSinceRequest = false;
            return DelayFrom(now);
        }
    }

    private TimeSpan DelayFrom(DateTimeOffset now)
    {
        var elapsed = now - _last;
        return elapsed >= interval ? TimeSpan.Zero : interval - elapsed;
    }
}
