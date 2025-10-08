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
        if (!request.IsValid)
            throw new ArgumentException("Arguments are missing.");

        // only returns filepath if upload is complete
        var uploadFilepathWhenCompleted = await uploader.EnsureChunk(request, config.Value);

        if (uploadFilepathWhenCompleted == null)
            return;

        EnqueueUnzippingJob(uploadFilepathWhenCompleted);
    }

    private void EnqueueUnzippingJob(string uploadFilepath)
    {
        var destinationDirectory = config.Value.UnzipDirectoryPath;

        if (string.IsNullOrWhiteSpace(destinationDirectory))
            throw new ArgumentException("Unzip directory is not configured.");

        tasks.QueueBackgroundWorkItem(async ct =>
            await unzipper.ExtractAsync(uploadFilepath, destinationDirectory, ct)
        );
    }
}
