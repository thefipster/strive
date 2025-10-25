using System.IO.Compression;
using Fip.Strive.Harvester.Application.Features.Expand.Component.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Models;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Features.Expand.Component;

public class Unzipper(IOptions<ExpandConfig> config) : IUnzipper
{
    private string TargetRootPath => config.Value.Path;

    public WorkItem Extract(WorkItem work, bool overwrite = false, CancellationToken ct = default)
    {
        var file = new FileInfo(work.ZipPath);
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

        ZipFile.ExtractToDirectory(work.ZipPath, outputPath, overwrite);
        work.OutputPath = outputPath;
        return work;
    }
}
