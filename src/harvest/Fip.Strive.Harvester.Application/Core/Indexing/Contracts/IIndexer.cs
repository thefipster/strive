namespace Fip.Strive.Harvester.Application.Core.Indexing.Contracts;

public interface IIndexer<TIndex, in TKey>
{
    TIndex? Find(TKey hash);
    void Upsert(TIndex index);
}
