using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Services.Contracts;
using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Queue.Application.Services.Contracts;

namespace Fip.Strive.Harvester.Application.Features.Expand.Services;

public class FileEmitter(
    IDirectoryService directoryService,
    IFileHasher hasher,
    IQueueService queue
) : IFileEmitter
{
    public async Task ScanFolderAsync(string path, ImportSignal signal, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidOperationException(
                $"Can't explore a null output path {path} for zip file {signal.Filepath}"
            );

        var files = directoryService.EnumerateAllFiles(path);
        foreach (var file in files)
        {
            var hash = await hasher.HashXx3Async(file, ct);
            var fileSignal = new FileSignal
            {
                Filepath = file,
                Hash = hash,
                ParentFilepath = signal.Filepath,
                ReferenceId = signal.ReferenceId,
            };
            await queue.EnqueueAsync(fileSignal, ct);
        }
    }
}
