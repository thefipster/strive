using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheFipster.ActivityAggregator.Storage.Lite.Configs;
using TheFipster.ActivityAggregator.Storage.Lite.Features.Indexing;

namespace TheFipster.ActivityAggregator.Storage.Lite;

public static class DependencyExtension
{
    public static void AddLiteDbStorage(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddConfiguration(configuration);
        services.AddIndexingFeature();
    }

    private static void AddConfiguration(
        this IServiceCollection services,
        IConfiguration configuration
    ) => services.Configure<LiteDbConfig>(configuration.GetSection(LiteDbConfig.ConfigSectionName));
}
