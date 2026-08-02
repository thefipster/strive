using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Mediators.Decorators;

public class BatchByDateValidator(IBatchByDateAction component) : IBatchByDateAction
{
    public IEnumerable<BatchIndex> GetBatchByDate(string date)
    {
        if (!DateTime.TryParse(date, out DateTime _))
            throw new ArgumentException("Invalid date format.", nameof(date));

        return component.GetBatchByDate(date);
    }
}
