using Fip.Strive.Harvester.Application.Features.Expand.Models;
using Fip.Strive.Harvester.Application.Features.Expand.Services.Contracts;
using Fip.Strive.Indexing.Domain;
using Fip.Strive.Queue.Application.Components.Contracts;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Harvester.Application.Features.Expand.Services.Decorators;

public class FileHashGateSignalled(
    IFileHashGate component,
    ISignalQueue queue,
    ILogger<FileHashGateSignalled> logger
) : IFileHashGate
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
