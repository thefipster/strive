using Fip.Strive.Indexing.Application.Contexts;
using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Indexing.Storage.Contracts;

namespace Fip.Strive.Indexing.Application.Repositories;

public class ZipRepository(ZipContext context) : IIndexerV2<ZipIndexV2, string>
{
    public Task<bool> ExistsAsync(string hash)
    {
        var result = context.Archives.Any(x => x.Hash == hash);
        return Task.FromResult(result);
    }

    public Task SetAsync(ZipIndexV2 index)
    {
        if (context.Archives.Any(x => x.Filepath == index.Filepath))
            throw new InvalidOperationException("Zip already exists.");

        context.Archives.Add(index);
        return Task.CompletedTask;
    }
}
