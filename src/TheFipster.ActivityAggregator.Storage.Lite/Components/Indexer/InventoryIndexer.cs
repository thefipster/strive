using LiteDB;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;
using TheFipster.ActivityAggregator.Storage.Lite.Context;

namespace TheFipster.ActivityAggregator.Storage.Lite.Components.Indexer;

public class InventoryIndexer(IndexerContext context)
    : BaseIndexer<InventoryIndex>(context),
        IInventoryIndexer
{
    public void EnsureIndex(InventoryIndex index)
    {
        var storedIndex = Collection.FindById(new BsonValue(index.Timestamp));
        if (storedIndex == null)
        {
            Collection.Insert(index);
            return;
        }

        storedIndex.Count++;
        Collection.Update(storedIndex);
    }

    public Dictionary<int, int[]> GetYearly()
    {
        var result = Collection
            .FindAll()
            .OrderBy(x => x.Timestamp)
            .GroupBy(e => e.Timestamp.Year)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    int dayCount = 0;
                    int sessionCount = 0;
                    int daySum = 0;
                    int sessionSum = 0;

                    foreach (var x in g)
                    {
                        if (x.IsDay)
                        {
                            dayCount++;
                            daySum += x.Count;
                        }
                        else
                        {
                            sessionCount++;
                            sessionSum += x.Count;
                        }
                    }

                    return new[] { dayCount, sessionCount, daySum, sessionSum };
                }
            );

        return result;
    }

    public PagedResponse<InventoryIndex> GetDaysPaged(
        int page,
        int size = 10,
        bool descending = false
    )
    {
        var query = Collection.Query();

        if (descending)
            query.OrderByDescending(x => x.Timestamp);
        else
            query.OrderBy(x => x.Timestamp);

        var count = query.Count();
        var items = query.Skip(page * size).Limit(size);

        return new PagedResponse<InventoryIndex>(items.ToList(), page, size, count);
    }

    public int GetMinYear() =>
        Collection.FindAll().OrderBy(x => x.Timestamp).First().Timestamp.Year;

    public IEnumerable<InventoryIndex> GetByDate(DateTime date)
    {
        var start = new DateTime(date.Year, date.Month, date.Day);
        var end = start.AddDays(1).AddMilliseconds(-1);
        return GetInRange(start, end);
    }

    public IEnumerable<InventoryIndex> GetByMonth(DateTime date)
    {
        var start = new DateTime(date.Year, date.Month, 1);
        var end = start.AddMonths(1).AddMilliseconds(-1);
        return GetInRange(start, end);
    }

    public IEnumerable<InventoryIndex> GetByYear(int year)
    {
        var start = new DateTime(year, 1, 1);
        var end = start.AddYears(1);
        return GetInRange(start, end);
    }

    public IEnumerable<InventoryIndex> GetInRange(DateTime start, DateTime end)
    {
        return Collection
            .Find(x => x.Timestamp >= start && x.Timestamp < end)
            .OrderByDescending(x => x.Timestamp)
            .ToList();
    }
}
