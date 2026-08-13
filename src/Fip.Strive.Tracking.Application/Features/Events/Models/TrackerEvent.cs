using Fip.Strive.Tracking.Application.Features.Trackers.Models;

namespace Fip.Strive.Tracking.Application.Features.Events.Models;

/// <summary>
/// One occurrence of the tracked thing: a timestamp, optionally a note, and a value per custom
/// field that was filled in.
/// </summary>
public class TrackerEvent
{
    public Guid Id { get; set; }

    public Guid TrackerId { get; set; }

    public Tracker? Tracker { get; set; }

    /// <summary>
    /// When it happened — user supplied, so it can be backdated. Always stored as UTC: SQLite keeps
    /// a <see cref="DateTimeOffset"/> as text and compares it lexically, which only sorts correctly
    /// while every row carries the same offset.
    /// </summary>
    public required DateTimeOffset OccurredUtc { get; set; }

    /// <summary>When the row was written. Diverges from <see cref="OccurredUtc"/> on backdating.</summary>
    public required DateTimeOffset RecordedUtc { get; set; }

    public string? Note { get; set; }

    public ICollection<TrackerEventValue> Values { get; set; } = [];
}
