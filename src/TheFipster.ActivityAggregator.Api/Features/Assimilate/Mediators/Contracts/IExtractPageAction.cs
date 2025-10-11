using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Mediators.Contracts;

public interface IExtractPageAction
{
    PagedResult<ExtractorIndex> GetFilePage(PagedRequest request);
}
