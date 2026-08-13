using Fip.Strive.Tracking.Application.Features.Trackers.Models;
using Fip.Strive.Tracking.Application.Features.Trackers.Services.Contracts;
using Fip.Strive.Tracking.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Tracking.Application.Features.Trackers.Services;

public sealed class TrackerReader(TrackingContext context) : ITrackerReader
{
    public async Task<IReadOnlyList<TrackerRow>> GetTrackersAsync(
        CancellationToken cancellationToken = default
    ) =>
        await context
            .Trackers.AsNoTracking()
            .OrderBy(tracker => tracker.Name)
            .Select(tracker => new TrackerRow(
                tracker.Id,
                tracker.Name,
                tracker.Description,
                tracker.Fields.Count,
                tracker.Fields.Count(field => field.IsRequired),
                tracker.Events.Count,
                // Cast to nullable so a tracker without events yields null instead of tripping over
                // an aggregate on an empty set.
                tracker.Events.Max(occurrence => (DateTimeOffset?)occurrence.OccurredUtc)
            ))
            .ToListAsync(cancellationToken);

    public async Task<TrackerDetail?> GetTrackerAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) =>
        await context
            .Trackers.AsNoTracking()
            .Where(tracker => tracker.Id == id)
            .Select(tracker => new TrackerDetail(
                tracker.Id,
                tracker.Name,
                tracker.Description,
                tracker.CreatedUtc,
                tracker
                    .Fields.OrderBy(field => field.SortOrder)
                    .ThenBy(field => field.Name)
                    .Select(field => new TrackerFieldRow(
                        field.Id,
                        field.Name,
                        field.Type,
                        field.Unit,
                        field.IsRequired,
                        field.SortOrder
                    ))
                    .ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);
}
