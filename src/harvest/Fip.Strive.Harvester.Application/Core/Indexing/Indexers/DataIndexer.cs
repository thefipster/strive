using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Application.Infrastructure.Contexts;
using LiteDB;

namespace Fip.Strive.Harvester.Application.Core.Indexing.Indexers;

public class DataIndexer : IIndexer<DataIndex, string>
{
    private readonly ILiteCollection<DataIndex> _collection;

    public DataIndexer(IndexContext context)
    {
        _collection = context.GetCollection<DataIndex>();
        _collection.EnsureIndex(x => x.Hash);
        _collection.EnsureIndex(x => x.ReferenceId);
    }

    public DataIndex? Find(string hash) => _collection.FindById(hash);

    public void Upsert(DataIndex index) => _collection.Upsert(index);
}
