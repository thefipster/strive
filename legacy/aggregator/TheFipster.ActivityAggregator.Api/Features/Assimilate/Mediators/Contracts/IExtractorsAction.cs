using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Mediators.Contracts;

public interface IExtractorsAction
{
    Dictionary<DataSources, int> GetExtractors();
}
