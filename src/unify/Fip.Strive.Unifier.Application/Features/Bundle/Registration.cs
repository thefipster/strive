using Fip.Strive.Unifier.Application.Features.Bundle.Services;
using Fip.Strive.Unifier.Application.Features.Bundle.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Unifier.Application.Features.Bundle;

internal sealed class UnifierApp;

public static class Registration
{
    public static void AddBundleFeature(this IServiceCollection services)
    {
        services.AddScoped<IBundleService, BundleService>();
    }
}
