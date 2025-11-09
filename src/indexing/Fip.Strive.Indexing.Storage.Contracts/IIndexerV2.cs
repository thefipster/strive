namespace Fip.Strive.Indexing.Storage.Contracts;

public interface IIndexerV2<TIndex, in TKey>
{
    Task<bool> ExistsAsync(TKey hash);
    Task SetAsync(TIndex index);
}
