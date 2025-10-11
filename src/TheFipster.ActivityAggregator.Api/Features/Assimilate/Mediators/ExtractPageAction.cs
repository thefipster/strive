using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Mediators;

public class ExtractPageAction(IPagedIndexer<ExtractorIndex> indexer) : IExtractPageAction
{
    public PagedResult<ExtractorIndex> GetFilePage(PagedRequest request)
    {
        var specifications = request.ToSpecification<ExtractorIndex>();
        return indexer.GetPaged(specifications);
    }
}
