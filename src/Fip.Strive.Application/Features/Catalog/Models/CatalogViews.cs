namespace Fip.Strive.Application.Features.Catalog.Models;

/// <param name="OccurrenceCount">
/// How many (package, path) pairs resolved to this content. Anything above one is dedup paying
/// for itself.
/// </param>
public sealed record CatalogEntryRow(
    string Hash,
    long SizeBytes,
    DateTimeOffset FirstSeenUtc,
    int OccurrenceCount
);

/// <summary>One place a blob showed up: which package, under which path.</summary>
public sealed record OccurrenceRow(
    Guid PackageId,
    string PackageName,
    DateTimeOffset UploadedUtc,
    string PathInArchive
);

public sealed record PackageRow(
    Guid Id,
    string FileName,
    string ArchiveHash,
    long SizeBytes,
    DateTimeOffset UploadedUtc,
    int FileCount,
    int NewEntryCount
)
{
    public int KnownEntryCount => FileCount - NewEntryCount;
}

/// <summary>Catalog totals for the import page.</summary>
public sealed record CatalogSummary(int PackageCount, int EntryCount, long TotalBytes);

public sealed record Page<T>(IReadOnlyList<T> Items, int TotalCount);
