namespace TheFipster.ActivityAggregator.Api.Features.Batch.Mediators.Contracts;

public interface IBatchExistsPerYearAction
{
    IEnumerable<DateTime> GetExists(int year);
}
