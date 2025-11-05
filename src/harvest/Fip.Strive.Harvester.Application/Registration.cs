using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Core.Application;
using Fip.Strive.Core.Ingestion;
using Fip.Strive.Harvester.Application.Core.Proxy;
using Fip.Strive.Harvester.Application.Core.Queue;
using Fip.Strive.Harvester.Application.Core.Schedule;
using Fip.Strive.Harvester.Application.Features.Assimilate;
using Fip.Strive.Harvester.Application.Features.Classify;
using Fip.Strive.Harvester.Application.Features.Expand;
using Fip.Strive.Harvester.Application.Features.Import;
using Fip.Strive.Harvester.Application.Features.Upload;
using Fip.Strive.Harvester.Application.Infrastructure;
using Fip.Strive.Indexing.Application;
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
        //services.AddLiteDbInfrastructure();
        services.AddPostgresInfrastructure(configuration);
        services.AddIngestionFeature();

        // Application Core
        services.AddInfrastructureServices();
        services.AddProxyFeature();
        services.AddScheduleFeature();
        services.AddQueueFeature();

        // Application Features
        services.AddUploadFeature();
        services.AddImportFeature();
        services.AddExpandFeature();
        services.AddClassifyFeature();
        services.AddAssimilateFeature();
    }
}
