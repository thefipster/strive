using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Services.Abstractions;

public interface IAssimilater
{
    AssimilaterIndex Standardize(ScannerIndex index);
}
