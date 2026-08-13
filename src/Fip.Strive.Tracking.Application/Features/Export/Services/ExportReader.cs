using Fip.Strive.Tracking.Application.Features.Export.Models;
using Fip.Strive.Tracking.Application.Features.Export.Services.Contracts;
using Fip.Strive.Tracking.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Tracking.Application.Features.Export.Services;

public sealed class ExportReader(TrackingContext context) : IExportReader
{
    public async Task<IReadOnlyList<TrackerExport>> GetTrackersAsync(
        CancellationToken cancellationToken = default
    ) =>
        await context
            .Trackers.AsNoTracking()
            .OrderBy(tracker => tracker.Name)
            .Select(tracker => new TrackerExport(
                tracker.Id,
                tracker.Name,
                tracker.Description,
                tracker.CreatedUtc,
                tracker
                    .Fields.OrderBy(field => field.SortOrder)
                    .Select(field => new FieldExport(
                        field.Id,
                        field.Name,
                        field.Type,
                        field.Unit,
                        field.IsRequired,
                        field.SortOrder
                    ))
                    .ToList()
            ))
            .ToListAsync(cancellationToken);

    public async Task<EventExportPage> GetEventsAsync(
        DateTimeOffset? since = null,
        Guid? trackerId = null,
        int skip = 0,
        int take = IExportReader.DefaultPageSize,
        CancellationToken cancellationToken = default
    )
    {
        var query = context.TrackerEvents.AsNoTracking();

        if (since is { } cursor)
        {
            // Inclusive, and on RecordedUtc rather than OccurredUtc: an event backdated to last
            // week is still news to a puller that synced yesterday.
            var floor = cursor.ToUniversalTime();
            query = query.Where(occurrence => occurrence.RecordedUtc >= floor);
        }

        if (trackerId is { } tracker)
            query = query.Where(occurrence => occurrence.TrackerId == tracker);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(occurrence => occurrence.RecordedUtc)
            .ThenBy(occurrence => occurrence.Id)
            .Skip(Math.Max(skip, 0))
            .Take(Math.Clamp(take, 1, IExportReader.MaximumPageSize))
            .Select(occurrence => new EventExport(
                occurrence.Id,
                occurrence.TrackerId,
                occurrence.Tracker!.Name,
                occurrence.OccurredUtc,
                occurrence.RecordedUtc,
                occurrence.Note,
                occurrence
                    .Values.OrderBy(value => value.Field!.SortOrder)
                    .Select(value => new ValueExport(
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

        var nextSince = items.Count > 0 ? items[^1].RecordedUtc : since;

        return new EventExportPage(items, totalCount, nextSince);
    }
}
