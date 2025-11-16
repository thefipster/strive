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
        var existing = context.Extractions.FirstOrDefault(x => x.Hash == index.Hash);

        if (existing == null)
            Insert(index);
        else
            Update(index, existing);

        return Task.CompletedTask;
    }

    private void Insert(ExtractIndexV2 index) => context.Extractions.Add(index);

    private void Update(ExtractIndexV2 index, ExtractIndexV2 existing)
    {
        existing.ParentHash = index.ParentHash;
        existing.Filepath = index.Filepath;
        existing.ParentFilepath = index.ParentFilepath;
        existing.Source = index.Source;
        existing.Kind = index.Kind;
        existing.Timestamp = index.Timestamp;
    }
}
