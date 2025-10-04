using System.Linq.Expressions;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

public interface IPagedIndexer<TIndex> : IIndexer<TIndex>
{
    PagedResult<TIndex> GetPaged(int page, int size);
    PagedResult<TIndex> GetPaged(int page, int size, Expression<Func<TIndex, bool>> filter);
    PagedResult<TIndex> GetPaged(
        int page,
        int size,
        Expression<Func<TIndex, bool>>? filter,
        Expression<Func<TIndex, object>> sort,
        bool descending = false
    );

    PagedResult<TIndex> GetPaged(
        int page,
        int size,
        Expression<Func<TIndex, bool>>? filter,
        Expression<Func<TIndex, object>> sort,
        bool descending,
        Expression<Func<TIndex, bool>> search
    );
}
