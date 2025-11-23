using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Core.Application;
using Fip.Strive.Harvester.Application.Core.Proxy;
using Fip.Strive.Harvester.Application.Core.Schedule;
using Fip.Strive.Harvester.Application.Features.Upload;
using Fip.Strive.Ingestion.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application;

internal sealed class HarvesterApp;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static void AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // System Core
        services.AddCoreServices<HarvesterApp>(configuration);
        services.AddIngestionFeature();

        // Application Core
        services.AddProxyFeature();
        services.AddScheduleFeature();

        // Application Features
        services.AddUploadFeature(configuration);
    }
}
