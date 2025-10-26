using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Models;
using Microsoft.Extensions.Options;
using IUnzipCoordinator = Fip.Strive.Harvester.Application.Features.Expand.Component.Contracts.IZipExtractor;

namespace Fip.Strive.Harvester.Application.Features.Expand.Component;

public class UnzipCoordinator(
    IOptions<ExpandConfig> config,
    IZipExtractor zipExtractor,
    IDirectoryService directoryService
) : IUnzipCoordinator
{
    private string TargetRootPath => config.Value.Path;

    public WorkItem Expand(WorkItem work, bool overwrite = false, CancellationToken ct = default)
    {
        var zipDirectoryName = Path.GetFileNameWithoutExtension(work.Signal.Filepath);
        var zipDirectoryPath = Path.Combine(TargetRootPath, zipDirectoryName);
        work.OutputPath = zipDirectoryPath;

        ThrowIfDirectoryCreationFails(work);
        ThrowIfExpansionFails(work, overwrite);

        return work;
    }

    private void ThrowIfDirectoryCreationFails(WorkItem work)
    {
        try
        {
            directoryService.CreateDirectory(TargetRootPath);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                $"Destination directory {work.OutputPath!} for zip file could not be created.",
                e
            );
        }
    }

    private void ThrowIfExpansionFails(WorkItem work, bool overwrite = false)
    {
        try
        {
            zipExtractor.ExtractToDirectory(work.Signal.Filepath, work.OutputPath!, overwrite);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                $"Zip file {work.OutputPath!} could not be expanded.",
                e
            );
        }
    }
}
