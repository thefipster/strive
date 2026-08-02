namespace Fip.Strive.Application.Features.Catalog.Models;

/// <summary>
/// One uploaded archive. The archive bytes themselves are not kept — every file inside is in the
/// blob store, so the archive is redundant once imported. <see cref="ArchiveHash"/> is what makes
/// re-uploading the same package a no-op.
/// </summary>
public class ImportPackage
{
    public Guid Id { get; set; }

    /// <summary>Lowercase hex SHA-256 of the archive bytes. Unique.</summary>
    public required string ArchiveHash { get; set; }

    /// <summary>Name the archive was uploaded under. Display only.</summary>
    public required string FileName { get; set; }

    public required long SizeBytes { get; set; }

    public required DateTimeOffset UploadedUtc { get; set; }

    /// <summary>Files contained in the archive, i.e. rows in <see cref="Files"/>.</summary>
    public required int FileCount { get; set; }

    /// <summary>How many of those files were content this catalog had never seen before.</summary>
    public required int NewEntryCount { get; set; }

    public ICollection<PackageFile> Files { get; set; } = [];
}
