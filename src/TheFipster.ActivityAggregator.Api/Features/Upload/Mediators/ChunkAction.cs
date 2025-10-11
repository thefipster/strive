using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Features.Core.Components.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Upload.Components.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Upload.Mediators.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Upload.Models;
using TheFipster.ActivityAggregator.Api.Features.Upload.Services.Contracts;
using TheFipster.ActivityAggregator.Domain.Configs;

namespace TheFipster.ActivityAggregator.Api.Features.Upload.Mediators;

public class ChunkAction(
    IOptions<ApiConfig> config,
    IBackgroundTaskQueue tasks,
    IUploader uploader,
    IUnzipService unzipper
) : IChunkAction
{
    public async Task UploadChunkAsync(UploadChunkRequest request)
    {
        // only returns filepath if upload is complete
        var uploadFilepathWhenCompleted = await uploader.EnsureChunkAsync(request);

        if (uploadFilepathWhenCompleted == null)
            return;

        tasks.QueueBackgroundWorkItem(async ct =>
            await unzipper.ExtractAsync(
                uploadFilepathWhenCompleted,
                config.Value.UnzipDirectoryPath,
                ct
            )
        );
    }
}
