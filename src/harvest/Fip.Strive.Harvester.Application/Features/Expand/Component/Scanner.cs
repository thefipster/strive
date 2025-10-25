using Fip.Strive.Harvester.Application.Features.Expand.Component.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Models;

namespace Fip.Strive.Harvester.Application.Features.Expand.Component;

public class Scanner(IChecker checker) : IScanner
{
    public async Task ExploreFolderAsync(WorkItem work, CancellationToken ct)
    {
        if (work.OutputPath == null)
            throw new InvalidOperationException(
                $"Can't explore a null output path for zip file {work.ZipPath}"
            );

        var files = Directory.EnumerateFiles(work.OutputPath, "*", SearchOption.AllDirectories);
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
