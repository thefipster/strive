using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Services.Abstractions;

public interface IUploader
{
    Task<string> EnsureChunk(UploadChunkRequest request, ApiConfig config);
}
