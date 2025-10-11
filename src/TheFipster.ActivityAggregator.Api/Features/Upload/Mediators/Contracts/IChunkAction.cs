using TheFipster.ActivityAggregator.Api.Features.Upload.Models;

namespace TheFipster.ActivityAggregator.Api.Features.Upload.Mediators.Contracts;

public interface IChunkAction
{
    Task UploadChunkAsync(UploadChunkRequest request);
}
