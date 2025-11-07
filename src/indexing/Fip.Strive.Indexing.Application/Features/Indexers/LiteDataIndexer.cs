using Fip.Strive.Indexing.Application.Features.Contracts;
using Fip.Strive.Indexing.Application.Infrastructure.Lite.Contexts;
using Fip.Strive.Indexing.Domain;
using LiteDB;

namespace Fip.Strive.Indexing.Application.Features.Indexers;

public class LiteDataIndexer : IIndexer<DataIndex, string>
{
    private readonly ILiteCollection<DataIndex> _collection;

    public LiteDataIndexer(IndexLiteContext context)
    {
        _collection = context.GetCollection<DataIndex>();
        _collection.EnsureIndex(x => x.Hash);
        _collection.EnsureIndex(x => x.ReferenceId);
    }

    public Task<DataIndex?> FindAsync(string hash)
    {
        var index = _collection.FindById(hash);
        return Task.FromResult<DataIndex?>(index);
    }

    public Task UpsertAsync(DataIndex index)
    {
        _collection.Upsert(index);
        return Task.CompletedTask;
    }
}
