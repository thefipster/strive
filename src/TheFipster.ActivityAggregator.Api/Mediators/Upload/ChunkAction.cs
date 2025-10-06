using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Api.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Exceptions;

namespace TheFipster.ActivityAggregator.Api.Mediators.Upload;

public class ChunkAction(
    IOptions<ApiConfig> config,
    IBackgroundTaskQueue tasks,
    IUploader uploader,
    IUnzipService unzipper
) : IChunkAction
{
    public async Task TryUploadChunkAsync(UploadChunkRequest request)
    {
        try
        {
            await HandleChunk(request);
        }
        catch (ArgumentException e)
        {
            throw new HttpResponseException(400, "Invalid arguments.", e.Message);
        }
        catch (Exception e)
        {
            throw new HttpResponseException(500, "Unexpected error occured.", e.Message);
        }
    }

    private async Task HandleChunk(UploadChunkRequest request)
    {
        var uploadFilepath = await uploader.EnsureChunk(request, config.Value);
        var destinationDirectory = config.Value.UnzipDirectoryPath;

        if (
            !string.IsNullOrWhiteSpace(uploadFilepath)
            && !string.IsNullOrWhiteSpace(destinationDirectory)
        )
        {
            tasks.QueueBackgroundWorkItem(async ct =>
                await unzipper.ExtractAsync(uploadFilepath, destinationDirectory, ct)
            );
        }
    }
}
