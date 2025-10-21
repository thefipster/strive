using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Setup.Configs;
using TheFipster.ActivityAggregator.Domain.Models.Importing;

namespace TheFipster.ActivityAggregator.Api.Features.Upload.Components.Decorators;

public class UnzipperValidator(IUnzipper component, IOptions<ImportConfig> config) : IUnzipper
{
    private string UnzipPath => config.Value.UnzipDirectoryPath;

    public DirectoryStats Extract(string zipFilePath, bool overwrite = false)
    {
        if (string.IsNullOrWhiteSpace(zipFilePath))
            throw new ArgumentException("Zip file path must not be empty.", nameof(zipFilePath));

        if (string.IsNullOrWhiteSpace(UnzipPath))
            throw new ArgumentException(
                "Destination directory must not be empty.",
                nameof(config.Value.UnzipDirectoryPath)
            );

        if (!File.Exists(zipFilePath))
            throw new FileNotFoundException("The zip file was not found.", zipFilePath);

        return component.Extract(zipFilePath, overwrite);
    }
}
