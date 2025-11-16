namespace Fip.Strive.Indexing.Storage.Contracts;

public interface IIndexer<TIndex, in TKey>
{
    Task<TIndex?> FindAsync(TKey key);
    Task UpsertAsync(TIndex index);
}
