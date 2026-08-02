namespace TheFipster.ActivityAggregator.Api.Features.Queue;

public static class ServiceExtension
{
    public static void AddQueueFeature(this IServiceCollection services)
    {
        services.AddHostedService<QueuedHostedService>();
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
    }
}
