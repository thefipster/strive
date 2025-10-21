using System.IO.Compression;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Setup.Configs;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Importing;

namespace TheFipster.ActivityAggregator.Api.Features.Upload.Components;

public class Unzipper(IOptions<ImportConfig> config) : IUnzipper
{
    private string UnzipPath => config.Value.UnzipDirectoryPath;

    public DirectoryStats Extract(string zipFilePath, bool overwrite = false)
    {
        var file = new FileInfo(zipFilePath);
        var outputName = file.Name.Replace(file.Extension, string.Empty);
        var outputPath = Path.Combine(UnzipPath, outputName);

        try
        {
            Directory.CreateDirectory(UnzipPath); // ensures target directory exists
        }
        catch (Exception e)
        {
            throw new ArgumentException(
                "Destination directory could not be created.",
                nameof(config.Value.UnzipDirectoryPath),
                e
            );
        }

        ZipFile.ExtractToDirectory(zipFilePath, outputPath, overwrite);
        return new DirectoryInfo(outputPath).GetFileCountAndSize();
    }
}
