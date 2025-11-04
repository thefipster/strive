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

    public ZipIndex? Find(string hash) => _collection.FindById(hash);

    public void Upsert(ZipIndex index) => _collection.Upsert(index);
}
