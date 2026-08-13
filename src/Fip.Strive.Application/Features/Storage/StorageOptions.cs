namespace Fip.Strive.Application.Features.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>
    /// Root of everything Strive writes to disk. A relative path resolves against the host's
    /// content root; an absolute path is used as-is. Override in a container with
    /// <c>Storage__DataDirectory</c>.
    /// </summary>
    public string DataDirectory { get; set; } = "data";

    /// <summary>Largest archive accepted by the upload page. Bounds the *compressed* upload.</summary>
    public long MaxUploadBytes { get; set; } = 8L * 1024 * 1024 * 1024;

    /// <summary>
    /// How many files an archive may contain. A vendor takeout of a decade of photos runs to a few
    /// hundred thousand, so this is set well above anything real rather than tightly.
    /// </summary>
    public int MaxArchiveEntries { get; set; } = 500_000;

    /// <summary>
    /// Largest single file an archive may unpack to. Above any plausible export, and low enough
    /// that one entry cannot fill a disk on its own.
    /// </summary>
    public long MaxEntryBytes { get; set; } = 4L * 1024 * 1024 * 1024;

    /// <summary>
    /// Largest total an archive may unpack to. This is the ceiling that a compression bomb actually
    /// meets: <see cref="MaxUploadBytes"/> bounds what arrives, and the ratio between the two is
    /// what an attacker gets for free.
    /// </summary>
    public long MaxTotalUncompressedBytes { get; set; } = 64L * 1024 * 1024 * 1024;
}
