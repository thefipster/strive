using Fip.Strive.Tracking.Application.Features.Events.Models;
using Fip.Strive.Tracking.Application.Features.Events.Services.Contracts;
using Fip.Strive.Tracking.Application.Features.Trackers.Models;
using Fip.Strive.Tracking.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Tracking.Application.Features.Events.Services;

public sealed class EventRecorder(TrackingContext context, TimeProvider clock) : IEventRecorder
{
    public async Task<Guid> RecordAsync(
        Guid trackerId,
        NewEvent entry,
        CancellationToken cancellationToken = default
    )
    {
        var exists = await context.Trackers.AnyAsync(
            tracker => tracker.Id == trackerId,
            cancellationToken
        );

        if (!exists)
            throw new TrackingException("That tracker no longer exists.");

        var fields = await context
            .TrackerFields.AsNoTracking()
            .Where(field => field.TrackerId == trackerId)
            .OrderBy(field => field.SortOrder)
            .ToListAsync(cancellationToken);

        var occurrence = new TrackerEvent
        {
            Id = Guid.NewGuid(),
            TrackerId = trackerId,
            // Normalised here rather than at the edge: every comparison and every ordering in this
            // app assumes a single offset, and callers hand over whatever their picker produced.
            OccurredUtc = entry.OccurredUtc.ToUniversalTime(),
            RecordedUtc = clock.GetUtcNow(),
            Note = Clean(entry.Note, TrackingLimits.NoteLength, "The note"),
            Values = BuildValues(fields, entry.Values ?? []),
        };

        context.TrackerEvents.Add(occurrence);
        await context.SaveChangesAsync(cancellationToken);

        return occurrence.Id;
    }

    public async Task DeleteAsync(Guid eventId, CancellationToken cancellationToken = default) =>
        await context
            .TrackerEvents.Where(occurrence => occurrence.Id == eventId)
            .ExecuteDeleteAsync(cancellationToken);

    /// <summary>
    /// Walks the tracker's fields rather than the submitted values, so anything sent for a field
    /// this tracker does not own is ignored instead of quietly becoming an orphaned row.
    /// </summary>
    private static List<TrackerEventValue> BuildValues(
        IReadOnlyList<TrackerField> fields,
        IReadOnlyList<NewEventValue> submitted
    )
    {
        var values = new List<TrackerEventValue>();

        foreach (var field in fields)
        {
            var supplied = submitted.FirstOrDefault(value => value.FieldId == field.Id);

            var number = field.Type == TrackerFieldType.Number ? supplied?.Number : null;
            var text =
                field.Type == TrackerFieldType.Text
                    ? Clean(supplied?.Text, TrackingLimits.TextValueLength, $"'{field.Name}'")
                    : null;

            if (number is null && text is null)
            {
                if (field.IsRequired)
                    throw new TrackingException($"'{field.Name}' needs a value.");

                // A blank optional field gets no row at all — nulls in a value table say nothing
                // that an absent row does not say more cheaply.
                continue;
            }

            values.Add(
                new TrackerEventValue
                {
                    Id = Guid.NewGuid(),
                    FieldId = field.Id,
                    Number = number,
                    Text = text,
                }
            );
        }

        return values;
    }

    private static string? Clean(string? value, int maxLength, string label)
    {
        var text = value?.Trim();

        if (string.IsNullOrEmpty(text))
            return null;

        if (text.Length > maxLength)
            throw new TrackingException($"{label} is limited to {maxLength} characters.");

        return text;
    }
}
