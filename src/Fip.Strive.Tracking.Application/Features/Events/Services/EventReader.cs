using Fip.Strive.Tracking.Application.Features.Events.Models;
using Fip.Strive.Tracking.Application.Features.Events.Services.Contracts;
using Fip.Strive.Tracking.Application.Features.Trackers.Models;
using Fip.Strive.Tracking.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Tracking.Application.Features.Events.Services;

public sealed class EventReader(TrackingContext context) : IEventReader
{
    public async Task<Page<EventRow>> GetEventsAsync(
        Guid trackerId,
        int skip,
        int take,
        CancellationToken cancellationToken = default
    )
    {
        var query = context
            .TrackerEvents.AsNoTracking()
            .Where(occurrence => occurrence.TrackerId == trackerId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(occurrence => occurrence.OccurredUtc)
            // Two events at the same instant would otherwise page unstably, dropping or repeating
            // rows as the user clicks through.
            .ThenByDescending(occurrence => occurrence.RecordedUtc)
            .ThenBy(occurrence => occurrence.Id)
            .Skip(skip)
            .Take(take)
            .Select(occurrence => new EventRow(
                occurrence.Id,
                occurrence.OccurredUtc,
                occurrence.Note,
                occurrence
                    .Values.OrderBy(value => value.Field!.SortOrder)
                    .Select(value => new EventValueRow(
                        value.FieldId,
                        value.Field!.Name,
                        value.Field.Type,
                        value.Field.Unit,
                        value.Number,
                        value.Text
                    ))
                    .ToList()
            ))
            .ToListAsync(cancellationToken);

        return new Page<EventRow>(items, totalCount);
    }

    public async Task<IReadOnlyList<NumberStats>> GetNumberStatsAsync(
        Guid trackerId,
        CancellationToken cancellationToken = default
    )
    {
        // Only the rows are fetched from SQLite; the arithmetic happens here. SQLite has no decimal
        // type — the provider stores one as text — so SUM and MIN in SQL would compare strings and
        // quietly return nonsense.
        var samples = await context
            .TrackerEventValues.AsNoTracking()
            .Where(value =>
                value.Field!.TrackerId == trackerId
                && value.Field.Type == TrackerFieldType.Number
                && value.Number != null
            )
            .Select(value => new NumberSample(
                value.FieldId,
                value.Field!.Name,
                value.Field.Unit,
                value.Field.SortOrder,
                value.Number
            ))
            .ToListAsync(cancellationToken);

        return samples
            .GroupBy(sample => new
            {
                sample.FieldId,
                sample.FieldName,
                sample.Unit,
                sample.SortOrder,
            })
            .OrderBy(group => group.Key.SortOrder)
            .Select(group =>
                Summarise(group.Key.FieldId, group.Key.FieldName, group.Key.Unit, group)
            )
            .ToList();
    }

    private static NumberStats Summarise(
        Guid fieldId,
        string fieldName,
        string? unit,
        IEnumerable<NumberSample> samples
    )
    {
        var numbers = samples.Select(sample => sample.Number).OfType<decimal>().ToList();

        return new NumberStats(
            fieldId,
            fieldName,
            unit,
            numbers.Count,
            numbers.Sum(),
            numbers.Min(),
            numbers.Max(),
            numbers.Average()
        );
    }

    /// <summary>One recorded number, flattened enough to group and total in memory.</summary>
    private sealed record NumberSample(
        Guid FieldId,
        string FieldName,
        string? Unit,
        int SortOrder,
        decimal? Number
    );
}
