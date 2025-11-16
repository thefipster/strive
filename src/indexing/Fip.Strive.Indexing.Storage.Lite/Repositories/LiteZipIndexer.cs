using Fip.Strive.Indexing.Domain;
using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Indexing.Storage.Contracts;
using Fip.Strive.Indexing.Storage.Lite.Contexts;
using LiteDB;

namespace Fip.Strive.Indexing.Storage.Lite.Repositories;

public class LiteZipIndexer : IIndexer<ZipIndex, string>
{
    private readonly ILiteCollection<ZipIndex> _collection;

    public LiteZipIndexer(IndexLiteContext context)
    {
        _collection = context.GetCollection<ZipIndex>();
        _collection.EnsureIndex(x => x.Hash);
        _collection.EnsureIndex(x => x.ReferenceId);
    }

    public Task<ZipIndex?> FindAsync(string key)
    {
        var result = _collection.FindById(key);
        return Task.FromResult<ZipIndex?>(result);
    }

    public Task UpsertAsync(ZipIndex index)
    {
        _collection.Upsert(index);
        return Task.CompletedTask;
    }
}
