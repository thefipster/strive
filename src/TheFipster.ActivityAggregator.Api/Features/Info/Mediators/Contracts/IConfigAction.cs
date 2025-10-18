namespace TheFipster.ActivityAggregator.Api.Features.Info.Mediators.Contracts;

public interface IConfigAction
{
    Dictionary<string, string> GetConfiguration();
}
