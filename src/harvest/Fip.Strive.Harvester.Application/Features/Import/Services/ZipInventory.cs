using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Core.Indexing.Repositories.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Components.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Models;
using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Harvester.Application.Features.Import.Services;

public class ZipInventory(
    IZipIndexer indexer,
    IZipFileAccess fileAccess,
    IFileHasher hasher,
    ILogger<ZipInventory> logger
) : IZipInventory
{
    public async Task<WorkItem> ImportAsync(UploadSignal signal, CancellationToken ct)
    {
        var work = WorkItem.FromSignal(signal);
        work.Hash = await hasher.HashXx3Async(work.Signal.Filepath, ct);

        work.Index = indexer.Find(work.Hash);
        if (FileIsIndexed(work))
            return RemoveAlreadyKnownFile(work);

        if (work.Index == null)
            work.ImportedPath = fileAccess.Import(work.Signal.Filepath);

        var index = work.ToIndex();
        indexer.Upsert(index);

        return work;
    }

    private bool FileIsIndexed(WorkItem work)
    {
        return work.Index != null && work.Index.Files.ContainsKey(work.Filename);
    }

    private WorkItem RemoveAlreadyKnownFile(WorkItem work)
    {
        logger.LogInformation(
            "File {UploadFile} is already imported and will be deleted.",
            work.Filename
        );

        work.Skip = true;
        File.Delete(work.Signal.Filepath);

        return work;
    }
}
