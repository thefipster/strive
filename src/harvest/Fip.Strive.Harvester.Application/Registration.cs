using Castle.DynamicProxy;
using Fip.Strive.Harvester.Application.Core.Proxy;
using Fip.Strive.Harvester.Application.Core.Queue;
using Fip.Strive.Harvester.Application.Features.Import;
using Fip.Strive.Harvester.Application.Features.Schedule;
using Fip.Strive.Harvester.Application.Features.Upload;
using Fip.Strive.Harvester.Application.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application;

public static class Registration
{
    public static void AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Core
        services.AddProxyFeature(configuration);
        services.AddQueueFeature(configuration);
        services.AddInfrastructureServices(configuration);

        // Feature
        services.AddScheduleFeature(configuration);
        services.AddUploadFeature(configuration);
        services.AddImportFeature(configuration);
    }
}
