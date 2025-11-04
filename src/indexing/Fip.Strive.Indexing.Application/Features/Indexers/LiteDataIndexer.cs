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

    public DataIndex? Find(string hash) => _collection.FindById(hash);

    public void Upsert(DataIndex index) => _collection.Upsert(index);
}
