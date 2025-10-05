using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

public interface IInventoryIndexer : IIndexer<InventoryIndex>
{
    void EnsureIndex(InventoryIndex index);
    Dictionary<int, int[]> GetYearly();
    IEnumerable<InventoryIndex> GetByYear(int year);
    PagedResult<InventoryIndex> GetDaysPaged(int page, int size = 10, bool descending = false);
    int GetMinYear();
}
