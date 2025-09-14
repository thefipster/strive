using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheFipster.ActivityAggregator.Pipeline.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Config;
using TheFipster.ActivityAggregator.Pipeline.Models;
using TheFipster.ActivityAggregator.Pipeline.Stages;

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
        services.Configure<ExtractorConfig>(
            configuration.GetSection(ExtractorConfig.ConfigSectionName)
        );

        services.AddSingleton<PipelineState>();
        services.AddScoped<IScannerStage, ScannerStage>();
        services.AddScoped<ITransfomerStage, TransformerStage>();
        services.AddScoped<IPipeline, Pipeline>();

        return services;
    }
}
