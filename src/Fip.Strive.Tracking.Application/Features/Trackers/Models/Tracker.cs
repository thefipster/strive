using Fip.Strive.Tracking.Application.Features.Events.Models;

namespace Fip.Strive.Tracking.Application.Features.Trackers.Models;

/// <summary>
/// A named thing worth counting — "Coffee", "Headache", "Gym". Everything else hangs off it: the
/// custom fields it collects and the timestamped events that accumulate under it.
/// </summary>
public class Tracker
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public required DateTimeOffset CreatedUtc { get; set; }

    public ICollection<TrackerField> Fields { get; set; } = [];

    public ICollection<TrackerEvent> Events { get; set; } = [];
}
