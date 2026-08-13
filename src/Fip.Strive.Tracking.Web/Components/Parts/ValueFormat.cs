using System.Globalization;
using Fip.Strive.Tracking.Application.Features.Events.Models;
using Fip.Strive.Tracking.Application.Features.Trackers.Models;

namespace Fip.Strive.Tracking.Web.Components.Parts;

public static class ValueFormat
{
    /// <summary>
    /// Trailing zeros are dropped: a field that happens to hold whole numbers should not read like
    /// an accounting report, and one holding 12.75 should not be rounded to 13.
    /// </summary>
    public static string Number(decimal value) =>
        value.ToString("0.####", CultureInfo.CurrentCulture);

    public static string WithUnit(decimal value, string? unit) =>
        string.IsNullOrWhiteSpace(unit) ? Number(value) : $"{Number(value)} {unit}";

    /// <summary>
    /// A single recorded value as it appears in the event table. Empty for a field that was left
    /// blank, and for one that did not exist yet when the event was recorded.
    /// </summary>
    public static string Cell(EventValueRow? value)
    {
        if (value is null)
            return string.Empty;

        if (value.Type != TrackerFieldType.Number)
            return value.Text ?? string.Empty;

        return value.Number is { } number ? WithUnit(number, value.Unit) : string.Empty;
    }

    /// <summary>The field's label, with its unit in brackets when it has one.</summary>
    public static string Label(TrackerFieldRow field) =>
        string.IsNullOrWhiteSpace(field.Unit) ? field.Name : $"{field.Name} ({field.Unit})";

    /// <summary>Entry-form label, marking required fields the way every other form does.</summary>
    public static string EntryLabel(TrackerFieldRow field) =>
        field.IsRequired ? $"{Label(field)} *" : Label(field);

    public static string Moment(DateTimeOffset instant) =>
        instant.ToLocalTime().ToString("g", CultureInfo.CurrentCulture);
}
