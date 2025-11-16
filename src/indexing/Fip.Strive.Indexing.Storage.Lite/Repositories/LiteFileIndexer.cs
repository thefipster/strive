using Fip.Strive.Indexing.Domain;
using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Indexing.Storage.Contracts;
using Fip.Strive.Indexing.Storage.Lite.Contexts;
using LiteDB;

namespace Fip.Strive.Indexing.Storage.Lite.Repositories;

public class LiteFileIndexer : IIndexer<FileIndex, string>
{
    private readonly ILiteCollection<FileIndex> _collection;

    public LiteFileIndexer(IndexLiteContext context)
    {
        _collection = context.GetCollection<FileIndex>();
        _collection.EnsureIndex(x => x.Hash);
        _collection.EnsureIndex(x => x.ReferenceId);
    }

    public Task<FileIndex?> FindAsync(string key)
    {
        var index = _collection.FindById(key);
        return Task.FromResult<FileIndex?>(index);
    }

    public Task UpsertAsync(FileIndex index)
    {
        _collection.Upsert(index);
        return Task.CompletedTask;
    }
}
