namespace Fip.Strive.Application.Features.Jobs.Models;

public sealed record JobRow(
    Guid Id,
    string Kind,
    string TargetKey,
    string ComponentId,
    int ComponentVersion,
    JobState State,
    int Attempts,
    string? Error,
    int? ProgressCurrent,
    int? ProgressTotal,
    string? ProgressNote,
    DateTimeOffset EnqueuedUtc,
    DateTimeOffset? StartedUtc,
    DateTimeOffset? FinishedUtc
)
{
    /// <summary>
    /// Takes the current time rather than reading a clock, so a running job's elapsed time comes
    /// from the caller's <c>TimeProvider</c> like every other timestamp in the app.
    /// </summary>
    public TimeSpan? DurationAt(DateTimeOffset now) =>
        StartedUtc is null ? null : (FinishedUtc ?? now) - StartedUtc;

    public double? Percent =>
        ProgressTotal is null or 0 ? null : 100d * (ProgressCurrent ?? 0) / ProgressTotal.Value;
}

public sealed record JobCounts(int Pending, int Running, int Succeeded, int Failed, int Stale);
