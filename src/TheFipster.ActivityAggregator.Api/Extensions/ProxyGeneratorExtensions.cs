using Castle.DynamicProxy;

namespace TheFipster.ActivityAggregator.Api.Extensions;

public static class ProxyGeneratorExtensions
{
    public static IServiceCollection AddInterceptedScoped<TInterface, TImpl>(
        this IServiceCollection services,
        params Type[] interceptorTypes
    )
        where TInterface : class
        where TImpl : class, TInterface
    {
        services.AddSingleton<ProxyGenerator>();

        // Register all interceptors so they can be resolved
        foreach (var type in interceptorTypes)
        {
            services.AddScoped(type);
        }

        services.AddScoped<TInterface>(sp =>
        {
            var generator = sp.GetRequiredService<ProxyGenerator>();
            var target = ActivatorUtilities.CreateInstance<TImpl>(sp);

            // Resolve all interceptors from DI
            var interceptors = interceptorTypes
                .Select(t => (IInterceptor)sp.GetRequiredService(t))
                .ToArray();

            return generator.CreateInterfaceProxyWithTarget<TInterface>(target, interceptors);
        });

        return services;
    }
}
