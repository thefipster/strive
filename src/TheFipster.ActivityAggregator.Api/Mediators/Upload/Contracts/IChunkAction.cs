using TheFipster.ActivityAggregator.Api.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Mediators.Upload.Contracts;

public interface IChunkAction
{
    Task UploadChunkAsync(UploadChunkRequest request);
}
