using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Features.Upload.Components.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Upload.Models;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Tools;

namespace TheFipster.ActivityAggregator.Api.Features.Upload.Components;

public class Uploader(IOptions<ApiConfig> config) : IUploader
{
    private static readonly ConcurrentDictionary<string, object> UploadLocks = new();

    public async Task<string?> EnsureChunkAsync(UploadChunkRequest request)
    {
        try
        {
            Directory.CreateDirectory(config.Value.UploadDirectoryPath);
            return await HandleChunkAsync(request);
        }
        finally
        {
            UploadLocks.TryRemove(request.UploadId, out _);
        }
    }

    private async Task<string?> HandleChunkAsync(UploadChunkRequest request)
    {
        var filename = FileHelper.SanitizeFileName(request.FileName);
        var tempFilePath = Path.Combine(
            config.Value.UploadDirectoryPath,
            request.UploadId + ".part"
        );

        UploadLocks.GetOrAdd(request.UploadId, _ => new object());
        await AppendChunkAsync(request, tempFilePath);
        if (request.ChunkIndex == request.TotalChunks - 1)
        {
            var finalFilePath = Path.Combine(config.Value.UploadDirectoryPath, filename);
            FinalizeFile(request, tempFilePath, finalFilePath);
            return finalFilePath;
        }

        return null;
    }

    private static async Task AppendChunkAsync(UploadChunkRequest request, string tempFilePath)
    {
        await using var stream = new FileStream(
            tempFilePath,
            FileMode.Append,
            FileAccess.Write,
            FileShare.None,
            81920,
            useAsync: true
        );

        await request.Chunk.CopyToAsync(stream);
        await stream.FlushAsync();
    }

    private void FinalizeFile(UploadChunkRequest request, string tempFilePath, string finalFilePath)
    {
        var fileInfo = new FileInfo(tempFilePath);
        if (fileInfo.Length != request.TotalSize)
            throw new DataMisalignedException(
                $"Uploaded size {fileInfo.Length} != expected {request.TotalSize}"
            );

        var targetPath = FileHelper.GetUniqueFilePath(finalFilePath);
        File.Move(tempFilePath, targetPath);
    }
}
