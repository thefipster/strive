using Fip.Strive.Indexing.Application.Features.Contracts;
using Fip.Strive.Indexing.Application.Infrastructure.Lite.Contexts;
using Fip.Strive.Indexing.Domain;
using LiteDB;

namespace Fip.Strive.Indexing.Application.Features.Indexers;

public class LiteZipIndexer : IIndexer<ZipIndex, string>
{
    private readonly ILiteCollection<ZipIndex> _collection;

    public LiteZipIndexer(IndexLiteContext context)
    {
        _collection = context.GetCollection<ZipIndex>();
        _collection.EnsureIndex(x => x.Hash);
        _collection.EnsureIndex(x => x.ReferenceId);
    }

    public Task<ZipIndex?> FindAsync(string hash)
    {
        var result = _collection.FindById(hash);
        return Task.FromResult<ZipIndex?>(result);
    }

    public Task UpsertAsync(ZipIndex index)
    {
        _collection.Upsert(index);
        return Task.CompletedTask;
    }
}
