using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;
using TheFipster.ActivityAggregator.Storage.Lite.Context;

namespace TheFipster.ActivityAggregator.Storage.Lite.Components.Indexer;

public class PagedIndexer<TItem>(IndexerContext context)
    : BaseIndexer<TItem>(context),
        IPagedIndexer<TItem>
{
    public PagedResult<TItem> GetPaged(int page, int size)
    {
        var query = Collection.Query();
        var count = query.Count();
        var items = query.Skip(page * size).Limit(size).ToList();

        return new PagedResult<TItem>(items, page, size, count);
    }
}
