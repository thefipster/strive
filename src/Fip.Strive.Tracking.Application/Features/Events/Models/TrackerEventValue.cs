using Fip.Strive.Tracking.Application.Features.Trackers.Models;

namespace Fip.Strive.Tracking.Application.Features.Events.Models;

/// <summary>
/// What one custom field held for one event. Exactly one of <see cref="Number"/> and
/// <see cref="Text"/> is set, decided by the field's <see cref="TrackerField.Type"/>; a blank
/// optional field produces no row at all rather than a row full of nulls.
/// </summary>
public class TrackerEventValue
{
    public Guid Id { get; set; }

    public Guid EventId { get; set; }

    public TrackerEvent? Event { get; set; }

    public Guid FieldId { get; set; }

    public TrackerField? Field { get; set; }

    /// <summary>
    /// Set for <see cref="TrackerFieldType.Number"/>. SQLite stores a decimal as text, so it is
    /// never aggregated or ordered in SQL — see <c>EventReader</c>.
    /// </summary>
    public decimal? Number { get; set; }

    /// <summary>Set for <see cref="TrackerFieldType.Text"/>.</summary>
    public string? Text { get; set; }
}
