using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Core.Application;
using Fip.Strive.Harvester.Application.Core.Proxy;
using Fip.Strive.Harvester.Application.Core.Schedule;
using Fip.Strive.Harvester.Application.Features.Assimilate;
using Fip.Strive.Harvester.Application.Features.Classify;
using Fip.Strive.Harvester.Application.Features.Expand;
using Fip.Strive.Harvester.Application.Features.Import;
using Fip.Strive.Harvester.Application.Features.Upload;
using Fip.Strive.Indexing.Application;
using Fip.Strive.Ingestion.Application;
using Fip.Strive.Queue.Application;
using Fip.Strive.Queue.Storage.Lite;
using Fip.Strive.Queue.Storage.Memory;
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
        services.AddProxyFeature();
        services.AddScheduleFeature();
        services.AddQueueFeature<HarvesterApp>(configuration).WithLiteDbStorage();

        // Application Features
        services.AddUploadFeature();
        services.AddImportFeature();
        services.AddExpandFeature();
        services.AddClassifyFeature();
        services.AddAssimilateFeature();
    }
}
