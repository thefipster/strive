using System.IO.Compression;
using Fip.Strive.Application.Features.Catalog.Models;
using Fip.Strive.Application.Features.Import.Models;
using Fip.Strive.Application.Features.Import.Services.Contracts;
using Fip.Strive.Application.Features.Storage;
using Fip.Strive.Application.Features.Storage.Models;
using Fip.Strive.Application.Features.Storage.Services.Contracts;
using Fip.Strive.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Application.Features.Import.Services;

public sealed class PackageImporter(
    StriveContext context,
    IBlobStore blobStore,
    TimeProvider timeProvider,
    IOptions<StorageOptions> storage,
    ILogger<PackageImporter> logger
) : IPackageImporter
{
    public async Task<ImportResult> ImportAsync(
        StagedArchive archive,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        var existing = await context
            .ImportPackages.AsNoTracking()
            .FirstOrDefaultAsync(package => package.ArchiveHash == archive.Hash, cancellationToken);

        if (existing is not null)
        {
            logger.LogInformation(
                "Archive {ArchiveHash} was already imported as package {PackageId}; nothing to do",
                archive.Hash,
                existing.Id
            );

            return new ImportResult(
                ImportOutcome.DuplicateArchive,
                existing.Id,
                existing.FileCount,
                NewEntryCount: 0
            );
        }

        var manifest = await UnpackAsync(archive, progress, cancellationToken);
        return await RecordAsync(archive, manifest, cancellationToken);
    }

    /// <summary>
    /// Streams every file in the archive into the blob store. Nothing is extracted to an
    /// archive-supplied path — blobs are filed under their own hash — so a crafted archive cannot
    /// escape the blob directory.
    /// </summary>
    private async Task<List<ManifestLine>> UnpackAsync(
        StagedArchive archive,
        IProgress<ImportProgress>? progress,
        CancellationToken cancellationToken
    )
    {
        using var zip = ZipFile.OpenRead(archive.Path);

        var entries = zip.Entries.Where(IsFile).ToList();

        // Before a single byte is written: MaxUploadBytes bounded what arrived, not what it
        // expands to.
        var guard = new ExpansionGuard(storage.Value);
        guard.CheckDeclared(entries);

        var manifest = new List<ManifestLine>(entries.Count);
        var seenPaths = new HashSet<string>(StringComparer.Ordinal);
        var processed = 0;

        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var path = NormalizePath(entry.FullName);

            // The ZIP format permits duplicate names. Keeping the first occurrence keeps the
            // manifest a faithful path -> content map instead of failing the whole import.
            if (!seenPaths.Add(path))
            {
                logger.LogWarning(
                    "Archive {FileName} contains {Path} more than once; keeping the first occurrence",
                    archive.FileName,
                    path
                );
                continue;
            }

            await using var content = entry.Open();

            // The declared length is attacker-supplied, so the pre-flight above is a courtesy
            // rather than a guarantee. This is the one that holds.
            var blob = await blobStore.WriteAsync(guard.Bound(entry, content), cancellationToken);
            guard.Written(blob.SizeBytes);

            manifest.Add(new ManifestLine(path, blob.Hash, blob.SizeBytes));
            progress?.Report(new ImportProgress(++processed, entries.Count, path));
        }

        return manifest;
    }

    /// <summary>
    /// Writes the catalog entries, the package and its manifest in a single transaction, so a
    /// package never exists half-catalogued. Blobs are already on disk at this point; if this
    /// fails they are simply unreferenced bytes that a retry deduplicates against.
    /// </summary>
    private async Task<ImportResult> RecordAsync(
        StagedArchive archive,
        List<ManifestLine> manifest,
        CancellationToken cancellationToken
    )
    {
        var now = timeProvider.GetUtcNow();
        var distinctContent = manifest.DistinctBy(line => line.Hash).ToList();
        var hashes = distinctContent.Select(line => line.Hash).ToList();

        var known = await context
            .CatalogEntries.Where(entry => hashes.Contains(entry.Hash))
            .Select(entry => entry.Hash)
            .ToListAsync(cancellationToken);

        var knownHashes = known.ToHashSet(StringComparer.Ordinal);

        var newEntries = distinctContent
            .Where(line => !knownHashes.Contains(line.Hash))
            .Select(line => new CatalogEntry
            {
                Hash = line.Hash,
                SizeBytes = line.SizeBytes,
                FirstSeenUtc = now,
            })
            .ToList();

        var package = new ImportPackage
        {
            Id = Guid.CreateVersion7(),
            ArchiveHash = archive.Hash,
            FileName = archive.FileName,
            SizeBytes = archive.SizeBytes,
            UploadedUtc = now,
            FileCount = manifest.Count,
            NewEntryCount = newEntries.Count,
            Files = manifest
                .Select(line => new PackageFile { PathInArchive = line.Path, Hash = line.Hash })
                .ToList(),
        };

        context.CatalogEntries.AddRange(newEntries);
        context.ImportPackages.Add(package);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Imported {FileName} as package {PackageId}: {FileCount} files, {NewEntryCount} new to the catalog",
            archive.FileName,
            package.Id,
            package.FileCount,
            package.NewEntryCount
        );

        return new ImportResult(
            ImportOutcome.Imported,
            package.Id,
            package.FileCount,
            package.NewEntryCount
        );
    }

    /// <summary>Directory entries carry no content and end in a separator.</summary>
    private static bool IsFile(ZipArchiveEntry entry) => !string.IsNullOrEmpty(entry.Name);

    private static string NormalizePath(string pathInArchive) => pathInArchive.Replace('\\', '/');

    private readonly record struct ManifestLine(string Path, string Hash, long SizeBytes);
}
