using Fip.Strive.Indexing.Domain;
using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Indexing.Storage.Contracts;
using Fip.Strive.Indexing.Storage.Lite.Contexts;
using LiteDB;

namespace Fip.Strive.Indexing.Storage.Lite.Repositories;

public class LiteDataIndexer : IIndexer<DataIndex, string>
{
    private readonly ILiteCollection<DataIndex> _collection;

    public LiteDataIndexer(IndexLiteContext context)
    {
        _collection = context.GetCollection<DataIndex>();
        _collection.EnsureIndex(x => x.Hash);
        _collection.EnsureIndex(x => x.ReferenceId);
    }

    public Task<DataIndex?> FindAsync(string key)
    {
        var index = _collection.FindById(key);
        return Task.FromResult<DataIndex?>(index);
    }

    public Task UpsertAsync(DataIndex index)
    {
        _collection.Upsert(index);
        return Task.CompletedTask;
    }
}
