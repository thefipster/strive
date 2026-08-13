using System.Buffers;
using System.Security.Cryptography;
using Fip.Strive.Application.Features.Storage.Models;
using Fip.Strive.Application.Features.Storage.Services.Contracts;

namespace Fip.Strive.Application.Features.Storage.Services;

/// <summary>
/// Filesystem blob store sharded by hash prefix (<c>ab/cd/abcd…</c>) to keep directory fan-out
/// manageable across hundreds of thousands of files.
/// </summary>
/// <remarks>
/// Nothing here ever deletes. Blobs are written before the catalog transaction commits, so a failed
/// or cancelled import leaves unreferenced bytes that a retry deduplicates against — deliberate, and
/// harmless while nothing removes packages.
///
/// It stops being harmless the day something does. Deleting a package cascades its
/// <c>package_files</c> rows and leaves both the <c>catalog_entries</c> and the blobs behind, and
/// there is no way to find an unreferenced blob short of walking the whole store against the
/// catalog. Treat a garbage collector as a prerequisite for package deletion rather than a
/// follow-up to it.
/// </remarks>
public sealed class BlobStore(StoragePaths paths) : IBlobStore
{
    private const int BufferSize = 128 * 1024;
    private const string TempDirectoryName = ".tmp";

    public async Task<BlobWriteResult> WriteAsync(
        Stream content,
        CancellationToken cancellationToken = default
    )
    {
        var tempDirectory = Path.Combine(paths.Blobs, TempDirectoryName);
        Directory.CreateDirectory(tempDirectory);
        var tempPath = Path.Combine(tempDirectory, Path.GetRandomFileName());

        try
        {
            var (hash, size) = await WriteToTempAsync(content, tempPath, cancellationToken);
            var wasWritten = Commit(tempPath, hash);
            return new BlobWriteResult(hash, size, wasWritten);
        }
        finally
        {
            // Committing moves the file away; anything still here is a discarded duplicate or the
            // debris of a failed write.
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    public string GetPath(string hash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hash);

        if (hash.Length < 4)
            throw new ArgumentException($"'{hash}' is not a valid blob hash.", nameof(hash));

        return Path.Combine(paths.Blobs, hash[..2], hash[2..4], hash);
    }

    public bool Exists(string hash) => File.Exists(GetPath(hash));

    public Stream OpenRead(string hash) =>
        new FileStream(
            GetPath(hash),
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            BufferSize,
            FileOptions.SequentialScan
        );

    private static async Task<(string Hash, long Size)> WriteToTempAsync(
        Stream content,
        string tempPath,
        CancellationToken cancellationToken
    )
    {
        using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        long size = 0;

        try
        {
            await using var target = new FileStream(
                tempPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                BufferSize,
                FileOptions.SequentialScan | FileOptions.Asynchronous
            );

            int read;
            while (
                (read = await content.ReadAsync(buffer.AsMemory(0, BufferSize), cancellationToken))
                > 0
            )
            {
                hasher.AppendData(buffer, 0, read);
                await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                size += read;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        return (Convert.ToHexStringLower(hasher.GetHashAndReset()), size);
    }

    /// <summary>
    /// Moves the temp file into its content-addressed home. Returns false when that blob already
    /// existed, which is the normal case for re-imported packages.
    /// </summary>
    private bool Commit(string tempPath, string hash)
    {
        var destination = GetPath(hash);

        if (File.Exists(destination))
            return false;

        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

        try
        {
            File.Move(tempPath, destination, overwrite: false);
            return true;
        }
        catch (IOException) when (File.Exists(destination))
        {
            // Lost a race with a concurrent write of identical content. The bytes are equal by
            // definition, so the blob that got there first is just as good.
            return false;
        }
    }
}
