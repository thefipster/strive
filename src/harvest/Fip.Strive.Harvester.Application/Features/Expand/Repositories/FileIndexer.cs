using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Harvester.Application.Features.Expand.Repositories.Contracts;
using Fip.Strive.Harvester.Application.Infrastructure.Contexts;
using LiteDB;

namespace Fip.Strive.Harvester.Application.Features.Expand.Repositories;

public class FileIndexer(IndexContext context) : IFileIndexer
{
    private readonly ILiteCollection<FileIndex> _collection = context.GetCollection<FileIndex>();

    public FileIndex? Find(string hash) => _collection.FindById(hash);

    public void Upsert(FileIndex index) => _collection.Upsert(index);
}
