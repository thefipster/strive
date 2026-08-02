namespace Fip.Strive.Application.Features.Catalog.Models;

/// <summary>
/// One unique blob in the L0 raw layer. Identity is the SHA-256 of the bytes, so the same file
/// appearing under many paths in many packages stays a single entry.
/// </summary>
public class CatalogEntry
{
    /// <summary>Lowercase hex SHA-256 of the file bytes.</summary>
    public required string Hash { get; set; }

    public required long SizeBytes { get; set; }

    /// <summary>When this content was first seen, in any package.</summary>
    public required DateTimeOffset FirstSeenUtc { get; set; }

    public ICollection<PackageFile> Occurrences { get; set; } = [];
}
