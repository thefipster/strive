using Fip.Strive.Harvester.Application.Core.Queue;
using Fip.Strive.Harvester.Application.Features.Scheduler;
using Fip.Strive.Harvester.Application.Features.Upload;
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
        services.AddQueueFeature(configuration);
        services.AddSchedulerFeature(configuration);
        services.AddUploadFeature(configuration);
    }
}
