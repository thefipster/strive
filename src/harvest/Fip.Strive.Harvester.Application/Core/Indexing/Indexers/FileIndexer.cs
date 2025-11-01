using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Application.Infrastructure.Contexts;
using LiteDB;

namespace Fip.Strive.Harvester.Application.Core.Indexing.Indexers;

public class FileIndexer : IIndexer<FileIndex, string>
{
    private readonly ILiteCollection<FileIndex> _collection;

    public FileIndexer(IndexContext context)
    {
        _collection = context.GetCollection<FileIndex>();
        _collection.EnsureIndex(x => x.Hash);
        _collection.EnsureIndex(x => x.ReferenceId);
    }

    public FileIndex? Find(string hash) => _collection.FindById(hash);

    public void Upsert(FileIndex index) => _collection.Upsert(index);
}
