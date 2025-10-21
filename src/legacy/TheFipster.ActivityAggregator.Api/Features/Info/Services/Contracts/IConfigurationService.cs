namespace TheFipster.ActivityAggregator.Api.Features.Info.Services.Contracts;

public interface IConfigurationService
{
    Dictionary<string, string> GetConfig();
}
