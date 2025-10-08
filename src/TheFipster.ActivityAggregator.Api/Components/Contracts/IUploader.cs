using TheFipster.ActivityAggregator.Api.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Configs;

namespace TheFipster.ActivityAggregator.Api.Components.Contracts;

public interface IUploader
{
    Task<string?> EnsureChunk(UploadChunkRequest request, ApiConfig config);
}
