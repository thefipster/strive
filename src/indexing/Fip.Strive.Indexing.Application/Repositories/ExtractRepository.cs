using Fip.Strive.Indexing.Application.Contexts;
using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Indexing.Storage.Contracts;

namespace Fip.Strive.Indexing.Application.Repositories;

public class ExtractRepository(ExtractContext context) : IIndexerV2<ExtractIndexV2, string>
{
    public Task<bool> ExistsAsync(string hash)
    {
        var result = context.Extractions.Any(x => x.Hash == hash);
        return Task.FromResult(result);
    }

    public Task SetAsync(ExtractIndexV2 index)
    {
        if (context.Extractions.Any(x => x.Filepath == index.Filepath))
            throw new InvalidOperationException("File already exists.");

        context.Extractions.Add(index);
        return Task.CompletedTask;
    }
}
