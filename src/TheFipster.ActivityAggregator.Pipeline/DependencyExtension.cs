using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheFipster.ActivityAggregator.Pipeline.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Config;
using TheFipster.ActivityAggregator.Pipeline.Models;
using TheFipster.ActivityAggregator.Pipeline.Pipelines;
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
        services.Configure<ImporterConfig>(
            configuration.GetSection(ImporterConfig.ConfigSectionName)
        );
        services.Configure<ScannerConfig>(
            configuration.GetSection(ScannerConfig.ConfigSectionName)
        );
        services.Configure<ClassifierConfig>(
            configuration.GetSection(ClassifierConfig.ConfigSectionName)
        );
        services.Configure<ExtractorConfig>(
            configuration.GetSection(ExtractorConfig.ConfigSectionName)
        );
        services.Configure<BundlerConfig>(
            configuration.GetSection(BundlerConfig.ConfigSectionName)
        );
        services.Configure<UnifierConfig>(
            configuration.GetSection(UnifierConfig.ConfigSectionName)
        );

        services.AddSingleton<PipelineState<IngesterPipeline>>();

        services.AddScoped<IImporterStage, ImporterStage>();

        services.AddScoped<IScannerStage, ScannerStage>();

        services.AddScoped<IClassifierStage, ClassifierStage>();

        services.AddScoped<ITransfomerStage, TransformerStage>();

        services.AddScoped<IIngesterPipeline, IngesterPipeline>();

        services.AddSingleton<PipelineState<MergerPipeline>>();

        services.AddScoped<IBundlerStage, BundlerStage>();

        services.AddScoped<IUnifierStage, UnifierStage>();

        services.AddScoped<IMergerPipeline, MergerPipeline>();

        return services;
    }
}
