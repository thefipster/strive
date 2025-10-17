using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

public interface IPagedIndexer<TIndex> : IIndexer<TIndex>
{
    PagedResponse<TIndex> GetPaged(int page, int size);
    PagedResponse<TIndex> GetPaged(PagedRequest paging);
    PagedResponse<TIndex> GetPaged(PageSpecificationRequest<TIndex> specifications);
}
