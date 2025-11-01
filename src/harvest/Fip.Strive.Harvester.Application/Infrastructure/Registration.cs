using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Harvester.Application.Infrastructure.Contexts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Infrastructure;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static void AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddSingleton<IndexContext>();
        services.AddSingleton<SignalQueueContext>();
    }
}
