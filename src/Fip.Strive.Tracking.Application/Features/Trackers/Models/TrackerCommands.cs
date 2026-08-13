namespace Fip.Strive.Tracking.Application.Features.Trackers.Models;

public sealed record NewTracker(string Name, string? Description = null);

public sealed record TrackerEdit(string Name, string? Description);

/// <param name="Type">Fixed for the lifetime of the field — see <see cref="TrackerField"/>.</param>
public sealed record NewField(
    string Name,
    TrackerFieldType Type,
    string? Unit = null,
    bool IsRequired = false
);

public sealed record FieldEdit(string Name, string? Unit, bool IsRequired);
