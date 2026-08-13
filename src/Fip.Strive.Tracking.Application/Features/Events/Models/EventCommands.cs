namespace Fip.Strive.Tracking.Application.Features.Events.Models;

/// <param name="OccurredUtc">
/// Converted to UTC on the way in, so callers may hand over whatever offset the picker produced.
/// </param>
/// <param name="Values">
/// May be partial: fields left out are treated as blank, and entries for fields the tracker does
/// not own are ignored rather than rejected.
/// </param>
public sealed record NewEvent(
    DateTimeOffset OccurredUtc,
    string? Note = null,
    IReadOnlyList<NewEventValue>? Values = null
);

/// <summary>
/// One field's contribution to an event. Only the member matching the field's type is read, so a
/// form can keep both around while the user switches things about.
/// </summary>
public sealed record NewEventValue(Guid FieldId, decimal? Number = null, string? Text = null);
