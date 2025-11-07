using Fip.Strive.Indexing.Domain;

namespace Fip.Strive.Indexing.Application.Features.Contracts;

public interface IIndexer<TIndex, in TKey>
{
    Task<TIndex?> FindAsync(TKey hash);
    Task UpsertAsync(TIndex index);
}
