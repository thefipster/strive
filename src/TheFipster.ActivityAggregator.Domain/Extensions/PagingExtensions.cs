using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Domain.Extensions;

public static class PagingExtensions
{
    public static PagedResult<T> ToPagedResult<T>(
        this IOrderedEnumerable<T> collection,
        PagedRequest paging
    )
    {
        var count = collection.Count();
        var items = collection.Skip(paging.Page * paging.Size).Take(paging.Size);
        return paging.ToResult(items, count);
    }
}
