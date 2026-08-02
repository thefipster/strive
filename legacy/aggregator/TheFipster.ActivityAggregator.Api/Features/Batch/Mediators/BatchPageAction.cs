using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Storage.Abstractions.Features.Indexing.Components;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Mediators;

public class BatchPageAction(IPagedIndexer<BatchIndex> indexer) : IBatchPageAction
{
    public PagedResponse<BatchIndex> GetPage(PagedRequest request)
    {
        var specification = request.ToSpecification<BatchIndex>();
        return indexer.GetPaged(specification);
    }
}
