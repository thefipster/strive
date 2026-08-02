using Fip.Strive.Application.Features.Storage.Models;

namespace Fip.Strive.Application.Features.Storage.Services.Contracts;

/// <summary>
/// The L0 raw layer: immutable, content-addressed file bytes on disk. Blobs are never mutated or
/// overwritten — identical content is written exactly once.
/// </summary>
public interface IBlobStore
{
    /// <summary>
    /// Streams <paramref name="content"/> to disk while hashing it, then files it under its own
    /// hash. If that blob already exists the freshly written copy is discarded.
    /// </summary>
    Task<BlobWriteResult> WriteAsync(Stream content, CancellationToken cancellationToken = default);

    /// <summary>Absolute path a blob with this hash occupies, whether or not it exists yet.</summary>
    string GetPath(string hash);

    bool Exists(string hash);

    Stream OpenRead(string hash);
}
