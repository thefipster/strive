using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheFipster.ActivityAggregator.Storage.Lite.Context;

namespace TheFipster.ActivityAggregator.Storage.Lite;

public static class DependencyExtension
{
    public static void AddLiteDbStorage(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<LiteDbConfig>(configuration.GetSection(LiteDbConfig.ConfigSectionName));
        services.AddSingleton<IndexerContext>();
    }
}
