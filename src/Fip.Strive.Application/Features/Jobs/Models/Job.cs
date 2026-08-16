namespace Fip.Strive.Application.Features.Jobs.Models;

/// <summary>
/// One row per work unit, not per run. Re-running a unit updates this row, which is what keeps the
/// table bounded by how much work there is rather than by how often it has been replayed.
/// </summary>
public class Job
{
    public Guid Id { get; set; }

    public string Kind { get; set; } = string.Empty;

    /// <summary>
    /// The unit's natural key within its kind — an archive hash for unpacking, a catalog hash for
    /// classification later. Unique together with <see cref="Kind"/>.
    /// </summary>
    public string TargetKey { get; set; } = string.Empty;

    public string ComponentId { get; set; } = string.Empty;

    /// <summary>Stamped at enqueue. Step 3 compares it against the registry to find stale units.</summary>
    public int ComponentVersion { get; set; }

    public JobState State { get; set; }

    public int Attempts { get; set; }

    /// <summary>Kind-specific JSON. Opaque to everything but the handler that wrote it.</summary>
    public string? Payload { get; set; }

    public string? Error { get; set; }

    public int? ProgressCurrent { get; set; }

    public int? ProgressTotal { get; set; }

    public string? ProgressNote { get; set; }

    public DateTimeOffset EnqueuedUtc { get; set; }

    public DateTimeOffset? StartedUtc { get; set; }

    public DateTimeOffset? FinishedUtc { get; set; }
}
