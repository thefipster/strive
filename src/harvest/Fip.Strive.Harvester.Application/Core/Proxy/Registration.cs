using Castle.Core.Interceptor;
using Castle.DynamicProxy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Core.Proxy;

public static class Registration
{
    public static void AddProxyFeature(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddSingleton<ProxyGenerator>();

        services.Scan(scan =>
            scan.FromAssembliesOf(typeof(Registration))
                .AddClasses(classes => classes.AssignableTo<IInterceptor>())
                .AsSelf()
                .WithScopedLifetime()
        );
    }
}
