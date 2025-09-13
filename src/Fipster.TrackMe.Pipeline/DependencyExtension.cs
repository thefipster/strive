using Fipster.TrackMe.Pipeline.Abstractions;
using Fipster.TrackMe.Pipeline.Config;
using Fipster.TrackMe.Pipeline.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fipster.TrackMe.Pipeline;

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
