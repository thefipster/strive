using Castle.DynamicProxy;

namespace TheFipster.ActivityAggregator.Api.Extensions;

public static class ProxyGeneratorExtensions
{
    public static IServiceCollection AddInterceptedScoped<TInterface, TService>(
        this IServiceCollection services,
        params Type[] interceptorTypes
    )
        where TInterface : class
        where TService : class, TInterface
    {
        services.AddSingleton<ProxyGenerator>();

        foreach (var type in interceptorTypes)
            services.AddScoped(type);

        services.AddScoped<TInterface>(sp =>
        {
            var generator = sp.GetRequiredService<ProxyGenerator>();
            var target = ActivatorUtilities.CreateInstance<TService>(sp);

            var interceptors = interceptorTypes
                .Select(t => (IInterceptor)sp.GetRequiredService(t))
                .ToArray();

            return generator.CreateInterfaceProxyWithTarget<TInterface>(target, interceptors);
        });

        return services;
    }
}
