using System.IO.Compression;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Features.Expand.Component.Contracts;

public class Unzipper(IOptions<ExpandConfig> config) : IUnzipper
{
    private string TargetRootPath => config.Value.Path;

    public string Extract(string zipFilePath, bool overwrite = false)
    {
        var file = new FileInfo(zipFilePath);
        var outputName = file.Name.Replace(file.Extension, string.Empty);
        var outputPath = Path.Combine(TargetRootPath, outputName);

        try
        {
            Directory.CreateDirectory(TargetRootPath); // ensures target directory exists
        }
        catch (Exception e)
        {
            throw new ArgumentException(
                "Destination directory could not be created.",
                nameof(TargetRootPath),
                e
            );
        }

        ZipFile.ExtractToDirectory(zipFilePath, outputPath, overwrite);
        return outputPath;
    }
}
