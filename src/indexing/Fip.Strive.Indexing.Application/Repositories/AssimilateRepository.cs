using Fip.Strive.Indexing.Application.Contexts;
using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Indexing.Storage.Contracts;

namespace Fip.Strive.Indexing.Application.Repositories;

public class AssimilateRepository(AssimilateContext context) : IIndexerV2<AssimilateIndexV2, string>
{
    public Task<bool> ExistsAsync(string hash)
    {
        var result = context.Assimilations.Any(x => x.Hash == hash);
        return Task.FromResult(result);
    }

    public Task SetAsync(AssimilateIndexV2 index)
    {
        if (context.Assimilations.Any(x => x.Filepath == index.Filepath))
            throw new InvalidOperationException("File already exists.");

        context.Assimilations.Add(index);
        return Task.CompletedTask;
    }
}
