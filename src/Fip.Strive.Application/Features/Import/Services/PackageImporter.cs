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
using Npgsql;

namespace Fip.Strive.Application.Features.Import.Services;

public sealed class PackageImporter(
    StriveContext context,
    IBlobStore blobStore,
    TimeProvider timeProvider,
    IOptions<StorageOptions> storage,
    ILogger<PackageImporter> logger
) : IPackageImporter
{
    /// <summary>
    /// Rows per insert round-trip. Large enough that a normal takeout is a handful of commands,
    /// small enough that one batch is never the thing that exhausts memory.
    /// </summary>
    private const int InsertBatchSize = 5_000;

    /// <summary>
    /// How many times to rebuild the catalog write after losing a race for a shared content hash.
    /// One retry settles the ordinary case, because the second pass reads the winner's entries as
    /// already known; the rest are headroom for an import unlucky enough to lose twice.
    /// </summary>
    private const int MaxRecordAttempts = 3;

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
    /// Records the import, retrying the write when it loses a race for content another importer
    /// was introducing at the same moment.
    /// </summary>
    /// <remarks>
    /// A unique violation here has two causes and they need opposite responses. Another importer
    /// may have committed this same archive, in which case there is nothing left to do and the
    /// winner is the answer. Or it may have committed a <em>catalog entry</em> this import also
    /// computed as new — both read <c>known</c> before either wrote, so both believed they were
    /// introducing the same content hash. That one is not a duplicate archive and must not be
    /// reported as one: it is a lost race, and the next pass reads the winner's entry as already
    /// known and writes the package this import was always owed.
    /// </remarks>
    private async Task<ImportResult> RecordAsync(
        StagedArchive archive,
        List<ManifestLine> manifest,
        CancellationToken cancellationToken
    )
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await TryRecordAsync(archive, manifest, cancellationToken);
            }
            catch (DbUpdateException exception) when (IsUniqueViolation(exception))
            {
                // The failed save left its rows in the tracker, and the transaction they belonged
                // to has rolled back out from under them.
                context.ChangeTracker.Clear();

                if (await DuplicateOfAsync(archive, cancellationToken) is { } duplicate)
                {
                    logger.LogInformation(
                        "Archive {ArchiveHash} was imported concurrently; reporting the winner",
                        archive.Hash
                    );

                    return duplicate;
                }

                if (attempt == MaxRecordAttempts)
                    throw;

                logger.LogInformation(
                    exception,
                    "Lost a catalog race importing {FileName}; rebuilding the write (attempt {Attempt} of {MaxAttempts})",
                    archive.FileName,
                    attempt,
                    MaxRecordAttempts
                );
            }
        }
    }

    /// <summary>
    /// Writes the catalog entries, the package and its manifest in a single transaction, so a
    /// package never exists half-catalogued. Blobs are already on disk at this point; if this
    /// fails they are simply unreferenced bytes that a retry deduplicates against.
    /// </summary>
    /// <remarks>
    /// The rows are added in batches, with the change tracker cleared between them. Building the
    /// whole manifest as one graph hung a tracked entity on every file in the archive — a takeout
    /// with a few hundred thousand files meant that many live objects and one enormous command.
    /// The transaction is explicit for exactly that reason: batching means several
    /// <c>SaveChangesAsync</c> calls, and without a transaction wrapped round them a failure
    /// half-way would leave precisely the half-catalogued package the original design was built to
    /// prevent. Everything the attempt read is re-read on the next one, which is what makes a
    /// retry after a lost race correct rather than merely another go at the same doomed write.
    /// </remarks>
    private async Task<ImportResult> TryRecordAsync(
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

        // Built without its Files: the manifest is inserted in batches below rather than as one
        // object graph hanging off this.
        var package = new ImportPackage
        {
            Id = Guid.CreateVersion7(),
            ArchiveHash = archive.Hash,
            FileName = archive.FileName,
            SizeBytes = archive.SizeBytes,
            UploadedUtc = now,
            FileCount = manifest.Count,
            NewEntryCount = newEntries.Count,
        };

        // Disposed on the way out of a failed attempt, which rolls it back before the caller
        // re-reads through this same context.
        await using var transaction = await context.Database.BeginTransactionAsync(
            cancellationToken
        );

        context.ImportPackages.Add(package);
        await context.SaveChangesAsync(cancellationToken);

        await InsertInBatchesAsync(newEntries, cancellationToken);

        await InsertInBatchesAsync(
            manifest.Select(line => new PackageFile
            {
                PackageId = package.Id,
                PathInArchive = line.Path,
                Hash = line.Hash,
            }),
            cancellationToken
        );

        await transaction.CommitAsync(cancellationToken);

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

    /// <summary>
    /// Adds rows a batch at a time, clearing the change tracker after each. The clear is what
    /// actually bounds the memory — without it every saved entity stays tracked for the life of the
    /// context and the batching only splits the commands.
    /// </summary>
    private async Task InsertInBatchesAsync<TEntity>(
        IEnumerable<TEntity> rows,
        CancellationToken cancellationToken
    )
        where TEntity : class
    {
        foreach (var batch in rows.Chunk(InsertBatchSize))
        {
            context.AddRange(batch);
            await context.SaveChangesAsync(cancellationToken);

            // Safe because nothing above holds a reference expecting to stay tracked: the package
            // is already written and only its Id is used afterwards.
            context.ChangeTracker.Clear();
        }
    }

    /// <summary>
    /// Postgres reports a broken unique index as SQLSTATE 23505. Matched on the state rather than
    /// the constraint name so a renamed index does not silently turn this back into a raw failure —
    /// which index broke is then established by reading, not by trusting a name. A foreign-key or
    /// not-null violation would be a bug here rather than a race, and still surfaces as one.
    /// </summary>
    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException
            is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    /// <summary>
    /// The package another writer created for these bytes, or <c>null</c> when the violation was
    /// not about this archive at all — which is the ordinary case when two imports share a file
    /// neither had seen before.
    /// </summary>
    private async Task<ImportResult?> DuplicateOfAsync(
        StagedArchive archive,
        CancellationToken cancellationToken
    )
    {
        var winner = await context
            .ImportPackages.AsNoTracking()
            .FirstOrDefaultAsync(package => package.ArchiveHash == archive.Hash, cancellationToken);

        return winner is null
            ? null
            : new ImportResult(
                ImportOutcome.DuplicateArchive,
                winner.Id,
                winner.FileCount,
                NewEntryCount: 0
            );
    }

    /// <summary>Directory entries carry no content and end in a separator.</summary>
    private static bool IsFile(ZipArchiveEntry entry) => !string.IsNullOrEmpty(entry.Name);

    private static string NormalizePath(string pathInArchive) => pathInArchive.Replace('\\', '/');

    private readonly record struct ManifestLine(string Path, string Hash, long SizeBytes);
}
