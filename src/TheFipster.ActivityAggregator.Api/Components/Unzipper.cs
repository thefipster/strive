using System.IO.Compression;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Components.Contracts;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Importing;

namespace TheFipster.ActivityAggregator.Api.Components;

public class Unzipper(IOptions<ApiConfig> config) : IUnzipper
{
    private string UnzipPath => config.Value.UnzipDirectoryPath;

    /// <summary>
    /// Extracts the contents of a zip file into the target directory.
    /// </summary>
    /// <param name="zipFilePath">The full path to the zip file.</param>
    /// <param name="overwrite">If files in the output directory should be overwritten.</param>
    public DirectoryStats Extract(string zipFilePath, bool overwrite = false)
    {
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

        ZipFile.ExtractToDirectory(zipFilePath, UnzipPath, overwrite);
        return new DirectoryInfo(UnzipPath).GetFileCountAndSize();
    }
}
