using LiteDB;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;
using TheFipster.ActivityAggregator.Storage.Lite.Context;

namespace TheFipster.ActivityAggregator.Storage.Lite.Components.Indexer;

public class PagedIndexer<TItem>(IndexerContext context)
    : BaseIndexer<TItem>(context),
        IPagedIndexer<TItem>
{
    public PagedResult<TItem> GetPaged(int page, int size) =>
        GetPaged(new PagedRequest(page, size));

    public PagedResult<TItem> GetPaged(PagedRequest paging)
    {
        var specifications = new PageSpecificationRequest<TItem>(paging);
        return GetPaged(specifications);
    }

    public PagedResult<TItem> GetPaged(PageSpecificationRequest<TItem> specifications)
    {
        var query = Collection.Query();

        query = AppendFilters(query, specifications);
        query = AppendSorting(query, specifications);

        var count = query.Count();
        var items = query
            .Skip(specifications.Page * specifications.Size)
            .Limit(specifications.Size)
            .ToList();

        return new PagedResult<TItem>(items, specifications.Page, specifications.Size, count);
    }

    private ILiteQueryable<TItem> AppendFilters(
        ILiteQueryable<TItem> query,
        PageSpecificationRequest<TItem> specifications
    )
    {
        foreach (var filter in specifications.Filters)
            query = query.Where(filter);
        return query;
    }

    private ILiteQueryable<TItem> AppendSorting(
        ILiteQueryable<TItem> query,
        PageSpecificationRequest<TItem> specifications
    )
    {
        if (specifications.Sort != null)
            if (specifications.IsDescending)
                query = query.OrderByDescending(specifications.Sort);
            else
                query = query.OrderBy(specifications.Sort);
        return query;
    }
}
