using Fip.Strive.Tracking.Application.Features.Events.Models;

namespace Fip.Strive.Tracking.Application.Features.Events.Services.Contracts;

public interface IEventRecorder
{
    /// <summary>
    /// Writes one event. Throws <see cref="TrackingException"/> when the tracker is gone or a
    /// required field was left blank.
    /// </summary>
    Task<Guid> RecordAsync(
        Guid trackerId,
        NewEvent entry,
        CancellationToken cancellationToken = default
    );

    /// <summary>Removes the event and its values. Deleting an unknown id is a no-op.</summary>
    Task DeleteAsync(Guid eventId, CancellationToken cancellationToken = default);
}
