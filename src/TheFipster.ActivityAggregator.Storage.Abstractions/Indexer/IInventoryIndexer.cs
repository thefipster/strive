using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

public interface IInventoryIndexer : IIndexer<InventoryIndex>
{
    void EnsureIndex(InventoryIndex index);
    Dictionary<int, int[]> GetYearly();
    IEnumerable<InventoryIndex> GetByYear(int year);
    int GetMinYear();
}
