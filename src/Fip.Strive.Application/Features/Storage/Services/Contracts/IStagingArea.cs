using Fip.Strive.Application.Features.Storage.Models;

namespace Fip.Strive.Application.Features.Storage.Services.Contracts;

/// <summary>
/// Holds uploaded archives on disk between upload and import. Nothing here is durable: once the
/// contained files are in the blob store the archive itself is redundant and gets discarded.
/// </summary>
public interface IStagingArea
{
    /// <summary>
    /// Streams <paramref name="content"/> to disk without buffering it in memory, hashing as it
    /// goes. <paramref name="progress"/> receives the running byte count.
    /// </summary>
    Task<StagedArchive> StageAsync(
        string fileName,
        Stream content,
        IProgress<long>? progress = null,
        CancellationToken cancellationToken = default
    );

    void Discard(StagedArchive archive);
}
