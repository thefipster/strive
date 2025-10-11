using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Mediators.Contracts;

public interface IBatchByDateAction
{
    IEnumerable<BatchIndex> GetBatchByDate(string date);
}
