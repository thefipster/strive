using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Mediators.Upload.Contracts;
using TheFipster.ActivityAggregator.Api.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Configs;

namespace TheFipster.ActivityAggregator.Api.Mediators.Upload.Decorators;

public class ChunkActionValidator(IChunkAction component, IOptions<ApiConfig> config) : IChunkAction
{
    public Task UploadChunkAsync(UploadChunkRequest request)
    {
        ValidateRequest(request);
        ValidateConfig();
        ValidateFileExtension(request);

        return component.UploadChunkAsync(request);
    }

    private static void ValidateRequest(UploadChunkRequest request)
    {
        // TODO Use fluent validations

        if (!request.IsValid)
            throw new ArgumentException("Arguments are missing.");
    }

    private void ValidateConfig()
    {
        var destinationDirectory = config.Value.UnzipDirectoryPath;

        if (string.IsNullOrWhiteSpace(destinationDirectory))
            throw new ArgumentException("Unzip directory is not configured.");
    }

    private void ValidateFileExtension(UploadChunkRequest request)
    {
        var ext = Path.GetExtension(request.FileName);
        if (
            !string.Equals(
                ext,
                config.Value.ImportFileExtensionFilter,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            throw new ArgumentException("Invalid file extension.", nameof(request.FileName));
        }
    }
}
