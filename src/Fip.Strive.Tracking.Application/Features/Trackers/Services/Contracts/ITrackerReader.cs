using Fip.Strive.Tracking.Application.Features.Trackers.Models;

namespace Fip.Strive.Tracking.Application.Features.Trackers.Services.Contracts;

public interface ITrackerReader
{
    /// <summary>Every tracker with its counts, alphabetically.</summary>
    Task<IReadOnlyList<TrackerRow>> GetTrackersAsync(CancellationToken cancellationToken = default);

    /// <summary>The tracker and its fields, or null when the id does not exist any more.</summary>
    Task<TrackerDetail?> GetTrackerAsync(Guid id, CancellationToken cancellationToken = default);
}
