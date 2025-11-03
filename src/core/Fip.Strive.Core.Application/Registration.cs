using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Core.Application.Features.Config;
using Fip.Strive.Core.Application.Features.FileSystem;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Core.Application;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static void AddCoreServices<TApp>(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddFileSystemFeature();
        services.AddConfigs<TApp>(configuration);
    }
}
