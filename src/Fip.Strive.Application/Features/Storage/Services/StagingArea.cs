using System.Buffers;
using System.Security.Cryptography;
using Fip.Strive.Application.Features.Storage.Models;
using Fip.Strive.Application.Features.Storage.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Application.Features.Storage.Services;

public sealed class StagingArea(StoragePaths paths, ILogger<StagingArea> logger) : IStagingArea
{
    private const int BufferSize = 128 * 1024;

    public async Task<StagedArchive> StageAsync(
        string fileName,
        Stream content,
        IProgress<long>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        Directory.CreateDirectory(paths.Incoming);
        var path = Path.Combine(paths.Incoming, $"{Guid.NewGuid():n}.zip");

        using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        long size = 0;

        try
        {
            await using (
                var target = new FileStream(
                    path,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    BufferSize,
                    FileOptions.SequentialScan | FileOptions.Asynchronous
                )
            )
            {
                int read;
                while (
                    (
                        read = await content.ReadAsync(
                            buffer.AsMemory(0, BufferSize),
                            cancellationToken
                        )
                    ) > 0
                )
                {
                    hasher.AppendData(buffer, 0, read);
                    await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                    size += read;
                    progress?.Report(size);
                }
            }
        }
        catch
        {
            TryDelete(path);
            throw;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        var hash = Convert.ToHexStringLower(hasher.GetHashAndReset());
        logger.LogInformation("Staged {FileName} ({Size} bytes) as {Hash}", fileName, size, hash);

        return new StagedArchive(fileName, path, hash, size);
    }

    public void Discard(StagedArchive archive) => TryDelete(archive.Path);

    private void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (IOException exception)
        {
            // Leftovers in the staging area cost disk, not correctness — the import already
            // succeeded or failed on its own terms.
            logger.LogWarning(exception, "Could not remove staged archive {Path}", path);
        }
    }
}
