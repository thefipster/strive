using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheFipster.ActivityAggregator.Pipeline.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Config;
using TheFipster.ActivityAggregator.Pipeline.Services;

namespace TheFipster.ActivityAggregator.Pipeline;

public static class DependencyExtension
{
    public static IServiceCollection AddPipeline(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<PipelineConfig>(
            configuration.GetSection(PipelineConfig.ConfigSectionName)
        );
        services.Configure<ScannerConfig>(
            configuration.GetSection(ScannerConfig.ConfigSectionName)
        );

        services.AddScoped<IScannerService, ScannerService>();
        services.AddScoped<IPipeline, Pipeline>();

        return services;
    }
}
