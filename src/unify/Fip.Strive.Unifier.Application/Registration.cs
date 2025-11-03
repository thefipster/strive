using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Core.Application;
using Fip.Strive.Unifier.Application.Features.Bundle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Unifier.Application;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static void AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // System Core
        services.AddCoreServices<UnifierApp>(configuration);

        // Application Core

        // Application Features
        services.AddBundleFeature();
    }
}
