using Fip.Strive.Tracking.Application.Features.Trackers.Models;

namespace Fip.Strive.Tracking.Application.Features.Events.Models;

public sealed record EventRow(
    Guid Id,
    DateTimeOffset OccurredUtc,
    string? Note,
    IReadOnlyList<EventValueRow> Values
)
{
    /// <summary>
    /// Null when the field was left blank, or when it was added after this event was recorded —
    /// the event table has a column per field either way.
    /// </summary>
    public EventValueRow? ValueFor(Guid fieldId) =>
        Values.FirstOrDefault(value => value.FieldId == fieldId);
}

public sealed record EventValueRow(
    Guid FieldId,
    string FieldName,
    TrackerFieldType Type,
    string? Unit,
    decimal? Number,
    string? Text
);

/// <summary>
/// What a number field adds up to across every event of its tracker. Computed in memory rather
/// than in SQL: SQLite stores a decimal as text, so <c>SUM</c> and <c>MIN</c> would compare
/// strings.
/// </summary>
public sealed record NumberStats(
    Guid FieldId,
    string FieldName,
    string? Unit,
    int Count,
    decimal Total,
    decimal Minimum,
    decimal Maximum,
    decimal Average
);

public sealed record Page<T>(IReadOnlyList<T> Items, int TotalCount);
