using Fip.Strive.Application.Features.Jobs.Models;

namespace Fip.Strive.Application.Features.Jobs.Services;

/// <summary>
/// Rate-limits a handler's progress reports on the way to the database. A handler reports as often
/// as is natural for it — once per file, for unpacking — and this decides how much of that is worth
/// a write.
/// </summary>
public sealed class ThrottledProgress(
    Func<JobProgress, Task> write,
    TimeSpan interval,
    TimeProvider clock
) : IProgress<JobProgress>
{
    private readonly Lock _gate = new();

    private DateTimeOffset _lastWrite = DateTimeOffset.MinValue;
    private JobProgress? _held;

    /// <summary>
    /// The tail of the write chain. Every write is queued behind the one before it rather than
    /// started on its own, for two reasons: two reports that both clear the throttle would
    /// otherwise race to the row and the older position could be the one that lands last, and
    /// <see cref="FlushAsync"/> needs something to wait on or a write can still be in flight when
    /// the job's terminal state is written.
    /// </summary>
    private Task _chain = Task.CompletedTask;

    public void Report(JobProgress value)
    {
        lock (_gate)
        {
            var now = clock.GetUtcNow();

            if (now - _lastWrite < interval)
            {
                // Held rather than dropped, so the flush can write the final position.
                _held = value;
                return;
            }

            _lastWrite = now;
            _held = null;
            _chain = AppendAsync(_chain, value);
        }
    }

    /// <summary>
    /// Writes whatever the throttle is holding and waits for every write this instance started,
    /// not just that one. Called before a job's terminal state is written, so the last position the
    /// UI shows is the real one.
    /// </summary>
    public async Task FlushAsync()
    {
        Task chain;

        lock (_gate)
        {
            if (_held is { } held)
            {
                _lastWrite = clock.GetUtcNow();
                _held = null;
                _chain = AppendAsync(_chain, held);
            }

            chain = _chain;
        }

        await chain;
    }

    /// <summary>
    /// Queues a write behind the previous one. <paramref name="previous"/> is safe to await
    /// unguarded because every link swallows its own failure.
    /// </summary>
    private async Task AppendAsync(Task previous, JobProgress value)
    {
        await previous;
        await WriteSafelyAsync(value);
    }

    /// <summary>
    /// Progress is advisory. Losing a position is not worth failing the job that reported it, and
    /// a throw on the write chain would fault every write queued behind it.
    /// </summary>
    private async Task WriteSafelyAsync(JobProgress value)
    {
        try
        {
            await write(value);
        }
        catch (Exception)
        {
            // The caller logs; there is nothing useful to do here.
        }
    }
}
