namespace TheFipster.ActivityAggregator.Api.Features.Info;

public static class ServiceExtension
{
    public static void AddInfoFeature(this IServiceCollection services)
    {
        services.AddScoped<IConfigAction, ConfigAction>();
        services.AddScoped<IConfigurationService, ConfigurationService>();
    }
}
