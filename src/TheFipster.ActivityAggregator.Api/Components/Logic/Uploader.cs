using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using TheFipster.ActivityAggregator.Api.Components.Contracts;
using TheFipster.ActivityAggregator.Api.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Configs;

namespace TheFipster.ActivityAggregator.Api.Components.Logic;

public class Uploader : IUploader
{
    private static readonly ConcurrentDictionary<string, object> UploadLocks = new();

    public async Task<string?> EnsureChunk(UploadChunkRequest request, ApiConfig config)
    {
        try
        {
            Directory.CreateDirectory(config.UploadDirectoryPath);
            return await HandleChunk(request, config);
        }
        finally
        {
            UploadLocks.TryRemove(request.UploadId, out _);
        }
    }

    private static async Task<string?> HandleChunk(UploadChunkRequest request, ApiConfig config)
    {
        ValidateRequest(request, config);

        var filename = SanitizeFileName(request.FileName);
        var tempFilePath = Path.Combine(config.UploadDirectoryPath, request.UploadId + ".part");

        UploadLocks.GetOrAdd(request.UploadId, _ => new object());
        await AppendChunk(request, tempFilePath);
        if (request.ChunkIndex == request.TotalChunks - 1)
        {
            var finalFilePath = Path.Combine(config.UploadDirectoryPath, filename);
            FinalizeFile(request, tempFilePath, finalFilePath);
            return finalFilePath;
        }

        return null;
    }

    private static void ValidateRequest(UploadChunkRequest request, ApiConfig config)
    {
        var ext = Path.GetExtension(request.FileName);
        if (
            !string.Equals(
                ext,
                config.ImportFileExtensionFilter,
                StringComparison.OrdinalIgnoreCase
            )
        )
            throw new ArgumentException("Invalid file extension.", nameof(request.FileName));
    }

    private static void FinalizeFile(
        UploadChunkRequest request,
        string tempFilePath,
        string finalFilePath
    )
    {
        var fileInfo = new FileInfo(tempFilePath);
        if (fileInfo.Length != request.TotalSize)
            throw new DataMisalignedException(
                $"Uploaded size {fileInfo.Length} != expected {request.TotalSize}"
            );

        var targetPath = GetUniqueFilePath(finalFilePath);
        File.Move(tempFilePath, targetPath);
    }

    private static async Task AppendChunk(UploadChunkRequest request, string tempFilePath)
    {
        await using (
            var stream = new FileStream(
                tempFilePath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true
            )
        )
        {
            await request.Chunk.CopyToAsync(stream);
            await stream.FlushAsync();
        }
    }

    private static string GetUniqueFilePath(string path)
    {
        if (!File.Exists(path))
            return path;
        var dir = Path.GetDirectoryName(path) ?? "";
        var name = Path.GetFileNameWithoutExtension(path);
        var ext = Path.GetExtension(path);
        int i = 1;
        string candidate;
        do
        {
            candidate = Path.Combine(dir, $"{name}({i++}){ext}");
        } while (File.Exists(candidate));
        return candidate;
    }

    private static string SanitizeFileName(string input)
    {
        var file = Path.GetFileName(input);
        file = Regex.Replace(file, @"[^A-Za-z0-9\._\-() ]+", "");
        return file;
    }
}
