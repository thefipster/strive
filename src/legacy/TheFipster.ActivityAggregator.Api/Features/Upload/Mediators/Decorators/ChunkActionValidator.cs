using FluentValidation;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Setup.Configs;
using TheFipster.ActivityAggregator.Domain.Exceptions;

namespace TheFipster.ActivityAggregator.Api.Features.Upload.Mediators.Decorators;

public class ChunkActionValidator(
    IChunkAction component,
    IValidator<UploadChunkRequest> validator,
    IOptions<ImportConfig> config
) : IChunkAction
{
    public async Task UploadChunkAsync(UploadChunkRequest request)
    {
        await ValidateRequestAsync(request);
        ValidateConfig();
        ValidateFileExtension(request);

        await component.UploadChunkAsync(request);
    }

    private async Task ValidateRequestAsync(UploadChunkRequest request)
    {
        var result = await validator.ValidateAsync(request);

        if (result.IsValid)
            return;

        throw new ValidationException(result.Errors);
    }

    private void ValidateConfig()
    {
        var destinationDirectory = config.Value.UnzipDirectoryPath;

        if (string.IsNullOrWhiteSpace(destinationDirectory))
            throw new ConfigException(
                nameof(ImportConfig.UnzipDirectoryPath),
                "Unzip directory is not configured."
            );
    }

    private void ValidateFileExtension(UploadChunkRequest request)
    {
        var ext = Path.GetExtension(request.FileName);
        var filter = config.Value.ImportFileExtensionFilter;

        if (ext != filter)
            throw new ArgumentException(
                $"Invalid file extension. Only {filter} is allowed.",
                nameof(request.FileName)
            );
    }
}
