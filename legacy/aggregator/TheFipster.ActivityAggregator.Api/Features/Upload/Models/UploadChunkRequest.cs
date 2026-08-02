namespace TheFipster.ActivityAggregator.Api.Features.Upload.Models;

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
}
