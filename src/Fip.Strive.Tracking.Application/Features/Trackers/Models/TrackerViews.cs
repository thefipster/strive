namespace Fip.Strive.Tracking.Application.Features.Trackers.Models;

/// <param name="LastEventUtc">Null until the tracker has its first event.</param>
public sealed record TrackerRow(
    Guid Id,
    string Name,
    string? Description,
    int FieldCount,
    int RequiredFieldCount,
    int EventCount,
    DateTimeOffset? LastEventUtc
)
{
    /// <summary>
    /// Whether one-click logging is possible at all — a required field means the entry form has to
    /// be filled in first.
    /// </summary>
    public bool NeedsInput => RequiredFieldCount > 0;
}

public sealed record TrackerDetail(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedUtc,
    IReadOnlyList<TrackerFieldRow> Fields
);

public sealed record TrackerFieldRow(
    Guid Id,
    string Name,
    TrackerFieldType Type,
    string? Unit,
    bool IsRequired,
    int SortOrder
);
