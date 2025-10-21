using TheFipster.ActivityAggregator.Api.Features.Info.Mediators.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Info.Services.Contracts;

namespace TheFipster.ActivityAggregator.Api.Features.Info.Mediators;

public class ConfigAction(IConfigurationService confiService) : IConfigAction
{
    public Dictionary<string, string> GetConfiguration() => confiService.GetConfig();
}
