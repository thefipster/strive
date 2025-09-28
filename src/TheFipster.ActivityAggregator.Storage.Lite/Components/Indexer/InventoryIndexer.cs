using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;
using TheFipster.ActivityAggregator.Storage.Lite.Context;

namespace TheFipster.ActivityAggregator.Storage.Lite.Components.Indexer;

public class InventoryIndexer(IndexerContext context)
    : BaseIndexer<InventoryIndex>(context),
        IInventoryIndexer
{
    public void EnsureIndex(InventoryIndex index) => Collection.Upsert(index);

    public Dictionary<int, int[]> GetYearly()
    {
        var all = Collection.FindAll();

        var result = all.GroupBy(e => e.Timestamp.Year)
            .ToDictionary(
                g => g.Key,
                g => new int[] { g.Count(e => e.IsDay), g.Count(e => !e.IsDay) }
            );

        return result;
    }

    public IEnumerable<InventoryIndex> GetByYear(int year)
    {
        var start = new DateTime(year, 1, 1);
        var end = start.AddYears(1);
        return Collection.Find(x => x.Timestamp >= start && x.Timestamp < end).ToList();
    }
}
