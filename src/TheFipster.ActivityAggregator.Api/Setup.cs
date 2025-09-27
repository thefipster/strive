using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Services.Abstractions;
using TheFipster.ActivityAggregator.Services.Components;

namespace TheFipster.ActivityAggregator.Api;

public static class Setup
{
    public static IServiceCollection AddCustom(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<ApiConfig>(configuration.GetSection(ApiConfig.ConfigSectionName));
        services.AddTransient<IUploader, Uploader>();

        return services;
    }
}
