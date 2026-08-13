using Fip.Strive.Tracking.Application.Features.Trackers.Models;

namespace Fip.Strive.Tracking.Application.Features.Trackers.Services.Contracts;

/// <summary>
/// Every method throws <see cref="TrackingException"/> when the request breaks a rule the user can
/// see — a blank or duplicate name, an id that no longer exists.
/// </summary>
public interface ITrackerWriter
{
    Task<Guid> CreateTrackerAsync(
        NewTracker tracker,
        CancellationToken cancellationToken = default
    );

    Task UpdateTrackerAsync(
        Guid trackerId,
        TrackerEdit edit,
        CancellationToken cancellationToken = default
    );

    /// <summary>Takes the tracker's fields and its whole history with it.</summary>
    Task DeleteTrackerAsync(Guid trackerId, CancellationToken cancellationToken = default);

    Task<Guid> AddFieldAsync(
        Guid trackerId,
        NewField field,
        CancellationToken cancellationToken = default
    );

    Task UpdateFieldAsync(
        Guid fieldId,
        FieldEdit edit,
        CancellationToken cancellationToken = default
    );

    /// <summary>Drops the field and every value already recorded under it.</summary>
    Task DeleteFieldAsync(Guid fieldId, CancellationToken cancellationToken = default);
}
