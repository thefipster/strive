using System.Buffers;
using System.Security.Cryptography;
using Fip.Strive.Application.Features.Storage.Models;
using Fip.Strive.Application.Features.Storage.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Application.Features.Storage.Services;

/// <summary>
/// Holds an uploaded archive until the job that unpacks it has finished with it.
/// </summary>
/// <remarks>
/// Staged files are content-addressed, like blobs: the upload lands on a temporary name and is
/// then moved to <c>incoming/&lt;hash&gt;.zip</c>, keeping whatever is already there. That is what
/// makes the same archive stage to the same path however many times it is uploaded, and it is the
/// reason nothing here needs a cleanup pass. A staged file has exactly one name, so a second
/// upload of the same bytes cannot orphan the first, and an upload whose job never got queued is
/// picked up and reused by the next attempt at the same archive rather than sitting alongside it.
/// The residue is therefore at most one file per distinct archive that failed to queue, not one
/// per attempt.
/// </remarks>
public sealed class StagingArea(StoragePaths paths, ILogger<StagingArea> logger) : IStagingArea
{
    private const int BufferSize = 128 * 1024;
    private const string TempDirectoryName = ".tmp";

    public async Task<StagedArchive> StageAsync(
        string fileName,
        Stream content,
        IProgress<long>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        // Under Incoming rather than beside it, so the job tests that count staged archives with a
        // non-recursive enumeration never see a half-written upload.
        var tempDirectory = Path.Combine(paths.Incoming, TempDirectoryName);
        Directory.CreateDirectory(tempDirectory);
        var tempPath = Path.Combine(tempDirectory, Path.GetRandomFileName());

        using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        long size = 0;

        try
        {
            await using (
                var target = new FileStream(
                    tempPath,
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

            var hash = Convert.ToHexStringLower(hasher.GetHashAndReset());
            var path = Commit(tempPath, hash);

            logger.LogInformation(
                "Staged {FileName} ({Size} bytes) as {Hash}",
                fileName,
                size,
                hash
            );

            return new StagedArchive(fileName, path, hash, size);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);

            // Committing moves the file away; anything still here is a duplicate of an archive
            // already staged, or the debris of an upload that failed part-way.
            TryDelete(tempPath);
        }
    }

    /// <summary>
    /// Moves the upload to its content-addressed name, or leaves the copy already there alone.
    /// Keeping the incumbent is not merely equivalent — the bytes are equal by definition, and a
    /// job may be reading it right now.
    /// </summary>
    private string Commit(string tempPath, string hash)
    {
        var destination = Path.Combine(paths.Incoming, $"{hash}.zip");

        if (File.Exists(destination))
            return destination;

        try
        {
            File.Move(tempPath, destination, overwrite: false);
        }
        catch (IOException) when (File.Exists(destination))
        {
            // Lost a race with a concurrent upload of the same archive. The winner's copy is this
            // one, byte for byte.
        }

        return destination;
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
