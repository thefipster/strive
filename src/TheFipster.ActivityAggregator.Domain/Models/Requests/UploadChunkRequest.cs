using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TheFipster.ActivityAggregator.Domain.Models.Requests;

public class UploadChunkRequest
{
    [FromForm(Name = "chunk")]
    public IFormFile Chunk { get; set; } = null!;

    [FromForm(Name = "uploadId")]
    public string UploadId { get; set; } = string.Empty;

    [FromForm(Name = "fileName")]
    public string FileName { get; set; } = string.Empty;

    [FromForm(Name = "chunkIndex")]
    public int ChunkIndex { get; set; }

    [FromForm(Name = "totalChunks")]
    public int TotalChunks { get; set; }

    [FromForm(Name = "totalSize")]
    public long TotalSize { get; set; }

    public bool IsValid =>
        Chunk.Length > 0
        && !string.IsNullOrEmpty(Chunk.Name)
        && !string.IsNullOrEmpty(UploadId)
        && !string.IsNullOrEmpty(FileName);
}
