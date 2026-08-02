using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Setup.Configs;

namespace TheFipster.ActivityAggregator.Api.Features.Upload.Mediators;

public class ChunkAction(
    IOptions<ImportConfig> config,
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

        tasks.Enqueue(async ct =>
            await unzipper.ExtractAsync(
                uploadFilepathWhenCompleted,
                config.Value.UnzipDirectoryPath,
                ct
            )
        );
    }
}
