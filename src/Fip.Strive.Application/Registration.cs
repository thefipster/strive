using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Application;

/// <summary>
/// Composition root for the application layer. Feature registrations are added here as the
/// roadmap steps land; for now it only establishes the seam the web host wires into.
/// </summary>
public static class Registration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
