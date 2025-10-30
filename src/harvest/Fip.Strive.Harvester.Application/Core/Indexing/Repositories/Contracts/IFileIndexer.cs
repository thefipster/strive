using Fip.Strive.Core.Domain.Schemas.Index.Models;

namespace Fip.Strive.Harvester.Application.Core.Indexing.Repositories.Contracts;

public interface IFileIndexer
{
    FileIndex? Find(string hash);
    void Upsert(FileIndex index);
}
