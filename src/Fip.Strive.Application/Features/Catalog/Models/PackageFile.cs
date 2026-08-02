using System.Diagnostics.CodeAnalysis;

namespace Fip.Strive.Application.Features.Catalog.Models;

/// <summary>
/// One row of a package's manifest: this path, in this archive, held this content. Every
/// occurrence is recorded even though the bytes are stored once.
/// </summary>
public class PackageFile
{
    public long Id { get; set; }

    public Guid PackageId { get; set; }

    /// <summary>Path as it appeared inside the archive, with forward slashes.</summary>
    public required string PathInArchive { get; set; }

    /// <summary>SHA-256 of the content — the <see cref="CatalogEntry"/> this path resolved to.</summary>
    public required string Hash { get; set; }

    [SuppressMessage("Usage", "CA2227", Justification = "EF Core navigation property.")]
    public ImportPackage? Package { get; set; }

    public CatalogEntry? Entry { get; set; }
}
