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

    /// <summary>Largest archive accepted by the upload page.</summary>
    public long MaxUploadBytes { get; set; } = 8L * 1024 * 1024 * 1024;
}
