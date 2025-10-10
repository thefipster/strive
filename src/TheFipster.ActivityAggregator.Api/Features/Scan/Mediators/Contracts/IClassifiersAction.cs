using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Api.Mediators.Scan.Contracts;

public interface IClassifiersAction
{
    Dictionary<DataSources, int> GetClassifiers();
}
