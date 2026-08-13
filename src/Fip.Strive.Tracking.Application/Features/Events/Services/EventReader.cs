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

    /// <summary>
    /// Aggregated by SQLite rather than in memory. The previous shape fetched every recorded number
    /// of a tracker and totalled them here, which meant opening a tracker's page cost a full scan of
    /// its values — and the page recomputes this after every single event it records.
    /// </summary>
    /// <remarks>
    /// The reason it was in memory was sound: SQLite has no decimal type, EF stores one as text, and
    /// SUM or MIN over text compares strings and returns nonsense. Casting to REAL first is what
    /// makes SQL aggregation correct — SQLite parses the text as a number and totals that.
    ///
    /// The trade is that aggregates are computed in double rather than decimal, so a very long run
    /// of values with many decimal places could drift in the last digits. For counts, minutes, reps
    /// and weights this is exact, and the individual values are still stored and displayed as the
    /// decimals they were entered as — only the summary crosses through double.
    ///
    /// Raw SQL because the cast is the entire point and no LINQ spelling of it survives translation.
    /// </remarks>
    public async Task<IReadOnlyList<NumberStats>> GetNumberStatsAsync(
        Guid trackerId,
        CancellationToken cancellationToken = default
    )
    {
        var rows = await context
            .Database.SqlQuery<NumberAggregate>(
                $"""
                SELECT  f."Id"                        AS "FieldId",
                        f."Name"                      AS "FieldName",
                        f."Unit"                      AS "Unit",
                        COUNT(v."Number")             AS "Count",
                        SUM(CAST(v."Number" AS REAL)) AS "Total",
                        MIN(CAST(v."Number" AS REAL)) AS "Minimum",
                        MAX(CAST(v."Number" AS REAL)) AS "Maximum",
                        AVG(CAST(v."Number" AS REAL)) AS "Average"
                FROM    "tracker_event_values" v
                JOIN    "tracker_fields" f ON f."Id" = v."FieldId"
                WHERE   f."TrackerId" = {trackerId}
                AND     f."Type" = {(int)TrackerFieldType.Number}
                AND     v."Number" IS NOT NULL
                GROUP BY f."Id", f."Name", f."Unit", f."SortOrder"
                ORDER BY f."SortOrder"
                """
            )
            .ToListAsync(cancellationToken);

        return rows.Select(row => row.ToStats()).ToList();
    }

    /// <summary>
    /// One row of the aggregate query. Doubles because that is what SQLite returns for an
    /// aggregate over a REAL cast; converted back at the edge so the view model stays decimal.
    /// </summary>
    private sealed record NumberAggregate(
        Guid FieldId,
        string FieldName,
        string? Unit,
        int Count,
        double Total,
        double Minimum,
        double Maximum,
        double Average
    )
    {
        public NumberStats ToStats() =>
            new(
                FieldId,
                FieldName,
                Unit,
                Count,
                (decimal)Total,
                (decimal)Minimum,
                (decimal)Maximum,
                (decimal)Average
            );
    }
}
