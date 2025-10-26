using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Harvester.Application.Features.Expand.Component.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Models;
using Fip.Strive.Harvester.Application.Features.Expand.Repositories.Contracts;

namespace Fip.Strive.Harvester.Application.Features.Expand.Component;

public class FileHashGate(IFileIndexer indexer, IFileHasher hasher) : IFileHashGate
{
    public async Task<FileIndex> CheckFileAsync(
        WorkItem work,
        string filepath,
        CancellationToken ct
    )
    {
        var filename = Path.GetFileName(filepath);
        var hash = await hasher.HashXx3Async(filepath, ct);

        var index = indexer.Find(hash);
        if (index == null)
            index = work.ToIndex(hash);

        index.AddFile(filename);
        indexer.Upsert(index);

        return index;
    }
}
