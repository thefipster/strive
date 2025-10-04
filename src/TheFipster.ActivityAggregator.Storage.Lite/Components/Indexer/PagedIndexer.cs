using System.Linq.Expressions;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
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

    public PagedResult<TItem> GetPaged(int page, int size, Expression<Func<TItem, bool>> filter)
    {
        var query = Collection.Query().Where(filter);
        var count = query.Count();
        var items = query.Skip(page * size).Limit(size).ToList();

        return new PagedResult<TItem>(items, page, size, count);
    }

    public PagedResult<TItem> GetPaged(
        int page,
        int size,
        Expression<Func<TItem, bool>>? filter,
        Expression<Func<TItem, object>> sort,
        bool descending = false
    )
    {
        var query = Collection.Query();

        if (filter != null)
            query = query.Where(filter);

        if (descending)
            query = query.OrderByDescending(sort);
        else
            query = query.OrderBy(sort);

        var count = query.Count();
        var items = query.Skip(page * size).Limit(size).ToList();

        return new PagedResult<TItem>(items, page, size, count);
    }

    public PagedResult<TItem> GetPaged(
        int page,
        int size,
        Expression<Func<TItem, bool>>? filter,
        Expression<Func<TItem, object>> sort,
        bool descending,
        Expression<Func<TItem, bool>> search
    )
    {
        var query = Collection.Query();
        query = query.Where(search);

        if (filter != null)
            query = query.Where(filter);

        if (descending)
            query = query.OrderByDescending(sort);
        else
            query = query.OrderBy(sort);

        var count = query.Count();
        var items = query.Skip(page * size).Limit(size).ToList();

        return new PagedResult<TItem>(items, page, size, count);
    }
}
