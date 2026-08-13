namespace Fip.Strive.Tracking.Application.Features.Trackers.Models;

/// <summary>
/// Persisted by its numeric value, so the names may change but the numbers may not.
/// </summary>
public enum TrackerFieldType
{
    /// <summary>A decimal number — a count, a weight, a duration, a price.</summary>
    Number = 1,

    /// <summary>Free text.</summary>
    Text = 2,
}
