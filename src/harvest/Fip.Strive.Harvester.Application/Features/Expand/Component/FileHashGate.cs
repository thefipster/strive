using Fip.Strive.Core.Domain.Extensions;
using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Harvester.Application.Features.Expand.Component.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Models;
using Fip.Strive.Harvester.Application.Features.Expand.Repositories.Contracts;

namespace Fip.Strive.Harvester.Application.Features.Expand.Component;

public class FileHashGate(IFileIndexer indexer) : IFileHashGate
{
    public async Task<FileIndex> CheckFileAsync(
        WorkItem work,
        string filepath,
        CancellationToken ct
    )
    {
        var file = new FileInfo(filepath);
        var hash = await file.HashXx3Async(ct);

        var index = indexer.Find(hash);
        if (index == null)
            index = work.ToIndex(hash);

        index.AddFile(file.Name);
        indexer.Upsert(index);

        return index;
    }
}
