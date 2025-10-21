using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Api.Features.Scan.Mediators.Contracts;

public interface IClassifiersAction
{
    Dictionary<DataSources, int> GetClassifiers();
}
