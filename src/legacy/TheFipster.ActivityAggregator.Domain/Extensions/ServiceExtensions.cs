using Microsoft.Extensions.DependencyInjection;

namespace TheFipster.ActivityAggregator.Domain.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddImplementations<TInterface>(
        this IServiceCollection services
    )
    {
        var assembly = typeof(TInterface).Assembly;
        var extractorTypes = assembly
            .GetTypes()
            .Where(t =>
                typeof(TInterface).IsAssignableFrom(t)
                && t is { IsInterface: false, IsAbstract: false }
            );

        foreach (var type in extractorTypes)
            services.AddSingleton(typeof(TInterface), type);

        return services;
    }
}
