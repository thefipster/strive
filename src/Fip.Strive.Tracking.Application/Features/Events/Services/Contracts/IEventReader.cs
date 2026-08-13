using Fip.Strive.Tracking.Application.Features.Events.Models;

namespace Fip.Strive.Tracking.Application.Features.Events.Services.Contracts;

public interface IEventReader
{
    /// <summary>A page of the tracker's events, newest first.</summary>
    Task<Page<EventRow>> GetEventsAsync(
        Guid trackerId,
        int skip,
        int take,
        CancellationToken cancellationToken = default
    );

    /// <summary>One entry per number field that has at least one recorded value.</summary>
    Task<IReadOnlyList<NumberStats>> GetNumberStatsAsync(
        Guid trackerId,
        CancellationToken cancellationToken = default
    );
}
