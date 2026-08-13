using Fip.Strive.Tracking.Application.Features.Events.Models;

namespace Fip.Strive.Tracking.Application.Features.Trackers.Models;

/// <summary>
/// An extra value a tracker collects with every event — "cups", "severity", "where". The type is
/// fixed at creation: changing it later would leave already recorded values stranded in the wrong
/// column, so the UI offers delete-and-recreate instead.
/// </summary>
public class TrackerField
{
    public Guid Id { get; set; }

    public Guid TrackerId { get; set; }

    public Tracker? Tracker { get; set; }

    public required string Name { get; set; }

    public required TrackerFieldType Type { get; set; }

    /// <summary>Optional suffix shown next to a number — "kg", "min", "km".</summary>
    public string? Unit { get; set; }

    /// <summary>When set, an event cannot be recorded without a value for this field.</summary>
    public bool IsRequired { get; set; }

    /// <summary>Position in the entry form and in the event table. Assigned on creation.</summary>
    public int SortOrder { get; set; }

    public ICollection<TrackerEventValue> Values { get; set; } = [];
}
