using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

public interface IPagedIndexer<TIndex> : IIndexer<TIndex>
{
    PagedResult<TIndex> GetPaged(int page, int size);
    PagedResult<TIndex> GetPaged(PagedRequest paging);
    PagedResult<TIndex> GetPaged(PageSpecificationRequest<TIndex> specifications);
}
