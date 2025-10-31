using Fip.Strive.Core.Domain.Schemas.Index.Models;

namespace Fip.Strive.Harvester.Application.Core.Indexing.Repositories.Contracts;

public interface IIndexer<TIndex, in TKey>
{
    TIndex? Find(TKey hash);
    void Upsert(TIndex index);
}
