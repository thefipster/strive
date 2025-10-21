using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Mediators.Contracts;

public interface IBatchPageAction
{
    PagedResponse<BatchIndex> GetPage(PagedRequest request);
}
