using Fip.Strive.Harvester.Application.Features.Import.Models;
using Fip.Strive.Harvester.Application.Features.Import.Repositories.Contracts;
using Fip.Strive.Harvester.Application.Infrastructure.Contexts;
using LiteDB;

namespace Fip.Strive.Harvester.Application.Features.Import.Repositories;

public class ZipIndexer : IZipIndexer
{
    private readonly ILiteCollection<ZipIndex> _collection;

    public ZipIndexer(IndexContext context)
    {
        _collection = context.GetCollection<ZipIndex>();
        _collection.EnsureIndex(x => x.Hash);
    }

    public ZipIndex? Get(string hash) => _collection.FindById(hash);

    public void Set(ZipIndex index) => _collection.Upsert(index);
}
