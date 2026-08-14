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
        }

        // Not awaited: IProgress.Report is void by contract, and a handler blocking on a database
        // write to say where it is would make reporting cost more than the work being reported.
        _ = WriteSafelyAsync(value);
    }

    /// <summary>
    /// Writes whatever the throttle is holding, and waits for it. Called before a job's terminal
    /// state is written, so the last position the UI shows is the real one.
    /// </summary>
    public async Task FlushAsync()
    {
        JobProgress? held;

        lock (_gate)
        {
            held = _held;
            _held = null;

            if (held is not null)
                _lastWrite = clock.GetUtcNow();
        }

        if (held is not null)
            await WriteSafelyAsync(held.Value);
    }

    /// <summary>
    /// Progress is advisory. Losing a position is not worth failing the job that reported it, and
    /// a throw on the fire-and-forget path would be unobserved anyway.
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
