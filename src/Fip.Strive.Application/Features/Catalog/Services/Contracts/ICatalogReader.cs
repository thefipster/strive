using Fip.Strive.Application.Features.Catalog.Models;

namespace Fip.Strive.Application.Features.Catalog.Services.Contracts;

public interface ICatalogReader
{
    Task<CatalogSummary> GetSummaryAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PackageRow>> GetPackagesAsync(CancellationToken cancellationToken = default);

    Task<Page<CatalogEntryRow>> GetEntriesAsync(
        int skip,
        int take,
        string? search = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<OccurrenceRow>> GetOccurrencesAsync(
        string hash,
        CancellationToken cancellationToken = default
    );
}
