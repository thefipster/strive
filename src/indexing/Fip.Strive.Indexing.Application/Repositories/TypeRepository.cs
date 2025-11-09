using Fip.Strive.Indexing.Application.Contexts;
using Fip.Strive.Indexing.Application.Repositories.Contracts;
using Fip.Strive.Indexing.Domain.Models;

namespace Fip.Strive.Indexing.Application.Repositories;

public class TypeRepository(TypeContext context) : ITypeRepository
{
    public Task<bool> ExistsAsync(string hash)
    {
        var result = context.Classifications.Any(x => x.Hash == hash);
        return Task.FromResult(result);
    }

    public Task<TypeIndexV2?> FindAsync(string hash)
    {
        var result = context.Classifications.FirstOrDefault(x => x.Hash == hash);
        return Task.FromResult<TypeIndexV2?>(result);
    }

    public Task<TypeMeta?> GetInfoAsync(string hash)
    {
        var result = context.Classifications.FirstOrDefault(x => x.Hash == hash);
        if (result == null)
            return Task.FromResult<TypeMeta?>(null);

        var meta = new TypeMeta
        {
            Hash = hash,
            Source = result.Source,
            Version = result.Version,
        };

        return Task.FromResult<TypeMeta?>(meta);
    }

    public Task SetAsync(TypeIndexV2 index)
    {
        var existing = context.Classifications.FirstOrDefault(x => x.Hash == index.Hash);

        if (existing == null)
            Insert(index);
        else
            Update(index, existing);

        return Task.CompletedTask;
    }

    private void Insert(TypeIndexV2 index) => context.Classifications.Add(index);

    private void Update(TypeIndexV2 index, TypeIndexV2 existing)
    {
        existing.Source = index.Source;
        existing.Version = index.Version;
        existing.Filepath = index.Filepath;
    }
}
