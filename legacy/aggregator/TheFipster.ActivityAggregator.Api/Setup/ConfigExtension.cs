using TheFipster.ActivityAggregator.Api.Setup.Configs;

namespace TheFipster.ActivityAggregator.Api.Setup;

public static class ConfigExtension
{
    public static void AddConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SettingsConfig>(
            configuration.GetSection(SettingsConfig.ConfigSectionName)
        );

        services.Configure<ImportConfig>(configuration.GetSection(ImportConfig.ConfigSectionName));
    }
}
