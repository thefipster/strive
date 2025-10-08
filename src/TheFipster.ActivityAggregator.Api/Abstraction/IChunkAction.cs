using TheFipster.ActivityAggregator.Api.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Abstraction;

public interface IChunkAction
{
    Task TryUploadChunkAsync(UploadChunkRequest request);
}
