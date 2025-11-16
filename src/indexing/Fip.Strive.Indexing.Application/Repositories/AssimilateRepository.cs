using Fip.Strive.Indexing.Application.Contexts;
using Fip.Strive.Indexing.Application.Repositories.Contracts;
using Fip.Strive.Indexing.Domain.Models;

namespace Fip.Strive.Indexing.Application.Repositories;

public class AssimilateRepository(AssimilateContext context) : IAssimilateRepository
{
    public Task<bool> ExistsAsync(string hash)
    {
        var result = context.Assimilations.Any(x => x.Hash == hash);
        return Task.FromResult(result);
    }

    public Task<AssimilateIndexV2?> FindAsync(string hash)
    {
        var result = context.Assimilations.FirstOrDefault(x => x.Hash == hash);
        return Task.FromResult(result);
    }

    public Task SetAsync(AssimilateIndexV2 index)
    {
        var existing = context.Assimilations.FirstOrDefault(x => x.Hash == index.Hash);

        if (existing == null)
            context.Assimilations.Add(index);
        else
        {
            existing.Source = index.Source;
            existing.Version = index.Version;
            existing.Filepath = index.Filepath;
        }

        return Task.CompletedTask;
    }
}
