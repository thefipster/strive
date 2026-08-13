using Fip.Strive.Tracking.Application.Features.Trackers.Models;

namespace Fip.Strive.Tracking.Application.Features.Export.Models;

/// <summary>
/// The shapes the API hands out. Kept apart from the view models the pages use on purpose: those
/// change whenever the UI wants something different, and this is a contract another machine parses.
/// </summary>
public sealed record TrackerExport(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedUtc,
    IReadOnlyList<FieldExport> Fields
);

public sealed record FieldExport(
    Guid Id,
    string Name,
    TrackerFieldType Type,
    string? Unit,
    bool IsRequired,
    int SortOrder
);

/// <param name="RecordedUtc">
/// When the row was written, which is what <c>since</c> filters on — a backdated event recorded
/// today has to reach a puller that already synced past its <see cref="OccurredUtc"/>.
/// </param>
public sealed record EventExport(
    Guid Id,
    Guid TrackerId,
    string TrackerName,
    DateTimeOffset OccurredUtc,
    DateTimeOffset RecordedUtc,
    string? Note,
    IReadOnlyList<ValueExport> Values
);

public sealed record ValueExport(
    Guid FieldId,
    string FieldName,
    TrackerFieldType Type,
    string? Unit,
    decimal? Number,
    string? Text
);

/// <param name="NextSince">
/// What to pass as <c>since</c> next time: the newest <see cref="EventExport.RecordedUtc"/> in this
/// page, or the caller's own cursor when the page is empty. The filter is inclusive, so the last
/// event of a page arrives again on the next pull — delivery is at-least-once and callers dedupe on
/// <see cref="EventExport.Id"/>.
/// </param>
public sealed record EventExportPage(
    IReadOnlyList<EventExport> Items,
    int TotalCount,
    DateTimeOffset? NextSince
);
