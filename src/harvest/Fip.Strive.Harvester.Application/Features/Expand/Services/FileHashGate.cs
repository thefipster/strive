using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Models;
using Fip.Strive.Harvester.Application.Features.Expand.Services.Contracts;
using Fip.Strive.Indexing.Application.Features.Contracts;
using Fip.Strive.Indexing.Domain;
using Fip.Strive.Indexing.Domain.Extensions;

namespace Fip.Strive.Harvester.Application.Features.Expand.Services;

public class FileHashGate(IIndexer<FileIndex, string> indexer, IFileHasher hasher) : IFileHashGate
{
    public async Task<FileIndex> CheckFileAsync(
        WorkItem work,
        string filepath,
        CancellationToken ct
    )
    {
        var filename = Path.GetFileName(filepath);
        var hash = await hasher.HashXx3Async(filepath, ct);

        var index = await indexer.FindAsync(hash);
        if (index == null)
            index = work.ToIndex(hash);

        index.Files.Add(filename, hash);
        await indexer.UpsertAsync(index);

        return index;
    }
}
