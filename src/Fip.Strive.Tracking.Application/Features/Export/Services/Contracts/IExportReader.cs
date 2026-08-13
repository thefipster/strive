using Fip.Strive.Tracking.Application.Features.Export.Models;

namespace Fip.Strive.Tracking.Application.Features.Export.Services.Contracts;

/// <summary>
/// The read side the homelab pulls with. Everything here is ordered oldest-first by
/// <c>RecordedUtc</c>, so a caller can walk the whole history forwards and stop wherever it likes.
/// </summary>
public interface IExportReader
{
    Task<IReadOnlyList<TrackerExport>> GetTrackersAsync(CancellationToken cancellationToken = default);

    /// <param name="since">Inclusive lower bound on <c>RecordedUtc</c>. Null pulls from the start.</param>
    /// <param name="take">Clamped to <see cref="MaximumPageSize"/>.</param>
    Task<EventExportPage> GetEventsAsync(
        DateTimeOffset? since = null,
        Guid? trackerId = null,
        int skip = 0,
        int take = DefaultPageSize,
        CancellationToken cancellationToken = default
    );

    const int DefaultPageSize = 200;

    const int MaximumPageSize = 1000;
}
