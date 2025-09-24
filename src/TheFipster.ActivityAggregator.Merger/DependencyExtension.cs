using Microsoft.Extensions.DependencyInjection;
using TheFipster.ActivityAggregator.Merger.Abstractions;
using TheFipster.ActivityAggregator.Merger.Services;

namespace TheFipster.ActivityAggregator.Merger;

public static class DependencyExtension
{
    public static IServiceCollection AddMergingServices(this IServiceCollection services)
    {
        services.AddTransient<IMergeService, MergeService>();

        return services;
    }
}
