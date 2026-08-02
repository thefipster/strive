using Fip.Strive.Application.Features.Catalog.Models;
using Fip.Strive.Application.Features.Catalog.Services.Contracts;
using Fip.Strive.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Application.Features.Catalog.Services;

public sealed class CatalogReader(StriveContext context) : ICatalogReader
{
    public async Task<CatalogSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var packageCount = await context.ImportPackages.CountAsync(cancellationToken);
        var entryCount = await context.CatalogEntries.CountAsync(cancellationToken);
        var totalBytes = await context
            .CatalogEntries.SumAsync(entry => (long?)entry.SizeBytes, cancellationToken);

        return new CatalogSummary(packageCount, entryCount, totalBytes ?? 0);
    }

    public async Task<IReadOnlyList<PackageRow>> GetPackagesAsync(
        CancellationToken cancellationToken = default
    ) =>
        await context
            .ImportPackages.AsNoTracking()
            .OrderByDescending(package => package.UploadedUtc)
            .Select(package => new PackageRow(
                package.Id,
                package.FileName,
                package.ArchiveHash,
                package.SizeBytes,
                package.UploadedUtc,
                package.FileCount,
                package.NewEntryCount
            ))
            .ToListAsync(cancellationToken);

    public async Task<Page<CatalogEntryRow>> GetEntriesAsync(
        int skip,
        int take,
        string? search = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = context.CatalogEntries.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            // Match either the blob's own hash or any path it was filed under, so the browser
            // works whether you paste a hash or remember a filename.
            query = query.Where(entry =>
                entry.Hash.StartsWith(term)
                || entry.Occurrences.Any(occurrence => occurrence.PathInArchive.Contains(term))
            );
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(entry => entry.FirstSeenUtc)
            .ThenBy(entry => entry.Hash)
            .Skip(skip)
            .Take(take)
            .Select(entry => new CatalogEntryRow(
                entry.Hash,
                entry.SizeBytes,
                entry.FirstSeenUtc,
                entry.Occurrences.Count
            ))
            .ToListAsync(cancellationToken);

        return new Page<CatalogEntryRow>(items, totalCount);
    }

    public async Task<IReadOnlyList<OccurrenceRow>> GetOccurrencesAsync(
        string hash,
        CancellationToken cancellationToken = default
    ) =>
        await context
            .PackageFiles.AsNoTracking()
            .Where(file => file.Hash == hash)
            .OrderBy(file => file.Package!.UploadedUtc)
            .ThenBy(file => file.PathInArchive)
            .Select(file => new OccurrenceRow(
                file.PackageId,
                file.Package!.FileName,
                file.Package.UploadedUtc,
                file.PathInArchive
            ))
            .ToListAsync(cancellationToken);
}
