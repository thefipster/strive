using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Component.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Models;

namespace Fip.Strive.Harvester.Application.Features.Expand.Component;

public class ZipEnumerator(IFileHashGate checker, IDirectoryService directoryService)
    : IZipEnumerator
{
    public async Task ExploreFolderAsync(WorkItem work, CancellationToken ct)
    {
        if (work.OutputPath == null)
            throw new InvalidOperationException(
                $"Can't explore a null output path for zip file {work.Signal.Filepath}"
            );

        var files = directoryService.EnumerateAllFiles(work.OutputPath);
        foreach (var file in files)
            await CheckFileWithCancellationAsync(work, file, ct);
    }

    private async Task CheckFileWithCancellationAsync(
        WorkItem work,
        string file,
        CancellationToken ct
    )
    {
        ct.ThrowIfCancellationRequested();
        await checker.CheckFileAsync(work, file, ct);
    }
}
