using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Components.Contracts;
using TheFipster.ActivityAggregator.Api.Mediators.Upload.Contracts;
using TheFipster.ActivityAggregator.Api.Models.Requests;
using TheFipster.ActivityAggregator.Api.Services.Contracts;
using TheFipster.ActivityAggregator.Domain.Configs;

namespace TheFipster.ActivityAggregator.Api.Mediators.Upload;

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
