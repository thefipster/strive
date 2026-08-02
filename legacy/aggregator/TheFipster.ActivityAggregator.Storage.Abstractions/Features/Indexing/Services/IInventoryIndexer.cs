using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Storage.Abstractions.Features.Indexing.Components;

namespace TheFipster.ActivityAggregator.Storage.Abstractions.Features.Indexing.Services;

public interface IInventoryIndexer : IIndexer<InventoryIndex>
{
    int GetMinYear();

    void EnsureIndex(InventoryIndex index);

    Dictionary<int, int[]> GetYearly();
    PagedResponse<InventoryIndex> GetDaysPaged(int page, int size = 10, bool descending = false);

    IEnumerable<InventoryIndex> GetByDate(DateTime dayDate);
    IEnumerable<InventoryIndex> GetByMonth(DateTime date);
    IEnumerable<InventoryIndex> GetByYear(int year);
    IEnumerable<InventoryIndex> GetInRange(DateTime start, DateTime end);
}
