using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Application.Infrastructure.Contexts;
using LiteDB;

namespace Fip.Strive.Harvester.Application.Core.Indexing.Indexers;

public class ZipIndexer : IIndexer<ZipIndex, string>
{
    private readonly ILiteCollection<ZipIndex> _collection;

    public ZipIndexer(IndexContext context)
    {
        _collection = context.GetCollection<ZipIndex>();
        _collection.EnsureIndex(x => x.Hash);
        _collection.EnsureIndex(x => x.ReferenceId);
    }

    public ZipIndex? Find(string hash) => _collection.FindById(hash);

    public void Upsert(ZipIndex index) => _collection.Upsert(index);
}
