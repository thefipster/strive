using TheFipster.ActivityAggregator.Api.Features.Core.Components;
using TheFipster.ActivityAggregator.Api.Features.Core.Components.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Core.Services;

namespace TheFipster.ActivityAggregator.Api.Features.Core;

public static class ServiceExtension
{
    public static void AddCoreFeature(this IServiceCollection services)
    {
        services.AddHostedService<QueuedHostedService>();
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

        services.AddScoped<INotifier, Notifier>();
    }
}
