using System.Diagnostics.CodeAnalysis;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Core.Proxy;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static void AddProxyFeature(this IServiceCollection services)
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
