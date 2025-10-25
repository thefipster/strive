using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Component.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Models;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Harvester.Application.Features.Expand.Component.Decorators;

public class CheckerSignalled(
    IChecker component,
    ISignalQueue queue,
    ILogger<CheckerSignalled> logger
) : IChecker
{
    public async Task<FileIndex> CheckFileAsync(
        WorkItem work,
        string filepath,
        CancellationToken ct
    )
    {
        var index = await component.CheckFileAsync(work, filepath, ct);

        if (index.Files.Count == 0)
            return LogImpossibleError(filepath, index);

        if (index.Files.Count > 1)
            return LogFileIsKnown(filepath, index);

        var signal = work.ToSignal(filepath);
        await queue.EnqueueAsync(signal, ct);

        return index;
    }

    private FileIndex LogFileIsKnown(string filepath, FileIndex index)
    {
        logger.LogInformation("File {ExpandedFile} is already known. Skipping.", filepath);
        return index;
    }

    private FileIndex LogImpossibleError(string filepath, FileIndex index)
    {
        logger.LogInformation(
            "This should not happen... I have a filepath {ExpandedFile} "
                + "but the file list in the index is empty... don't ask me mate, "
                + "something upstream must have gone wrong.",
            filepath
        );
        return index;
    }
}
