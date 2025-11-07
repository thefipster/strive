using Fip.Strive.Indexing.Application.Features.Contracts;
using Fip.Strive.Indexing.Application.Infrastructure.Lite.Contexts;
using Fip.Strive.Indexing.Domain;
using LiteDB;

namespace Fip.Strive.Indexing.Application.Features.Indexers;

public class LiteFileIndexer : IIndexer<FileIndex, string>
{
    private readonly ILiteCollection<FileIndex> _collection;

    public LiteFileIndexer(IndexLiteContext context)
    {
        _collection = context.GetCollection<FileIndex>();
        _collection.EnsureIndex(x => x.Hash);
        _collection.EnsureIndex(x => x.ReferenceId);
    }

    public Task<FileIndex?> FindAsync(string hash)
    {
        var index = _collection.FindById(hash);
        return Task.FromResult<FileIndex?>(index);
    }

    public Task UpsertAsync(FileIndex index)
    {
        _collection.Upsert(index);
        return Task.CompletedTask;
    }
}
