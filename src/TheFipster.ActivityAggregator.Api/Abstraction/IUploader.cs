using TheFipster.ActivityAggregator.Api.Models;
using TheFipster.ActivityAggregator.Domain.Configs;

namespace TheFipster.ActivityAggregator.Api.Abstraction;

public interface IUploader
{
    Task<string?> EnsureChunk(UploadChunkRequest request, ApiConfig config);
}
