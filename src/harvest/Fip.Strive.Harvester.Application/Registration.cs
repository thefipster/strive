using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Core.Application;
using Fip.Strive.Core.Ingestion;
using Fip.Strive.Harvester.Application.Core.Config;
using Fip.Strive.Harvester.Application.Core.Indexing;
using Fip.Strive.Harvester.Application.Core.Proxy;
using Fip.Strive.Harvester.Application.Core.Queue;
using Fip.Strive.Harvester.Application.Features.Assimilate;
using Fip.Strive.Harvester.Application.Features.Classify;
using Fip.Strive.Harvester.Application.Features.Expand;
using Fip.Strive.Harvester.Application.Features.Import;
using Fip.Strive.Harvester.Application.Features.Schedule;
using Fip.Strive.Harvester.Application.Features.Upload;
using Fip.Strive.Harvester.Application.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static void AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // System Core
        services.AddCoreServices();
        services.AddIngestionFeature();

        // Application Core
        services.AddInfrastructureServices(configuration);
        services.AddConfigs(configuration);
        services.AddProxyFeature();
        services.AddQueueFeature();
        services.AddIndexingFeature();

        // Application Features
        services.AddScheduleFeature(configuration);
        services.AddUploadFeature();
        services.AddImportFeature();
        services.AddExpandFeature();
        services.AddClassifyFeature();
        services.AddAssimilateFeature();
    }
}
