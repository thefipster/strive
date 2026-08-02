namespace TheFipster.ActivityAggregator.Api.Features.Batch.Mediators.Decorators;

public class BatchExistsPerYearValidator(IBatchExistsPerYearAction component)
    : IBatchExistsPerYearAction
{
    public IEnumerable<DateTime> GetExists(int year)
    {
        if (year > DateTime.UtcNow.Year)
            throw new ArgumentException("Year cannot be in the future.", nameof(year));

        if (year < 1)
            throw new ArgumentException("Year cannot be less than 1.", nameof(year));

        return component.GetExists(year);
    }
}
