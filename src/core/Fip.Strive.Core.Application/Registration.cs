using Fip.Strive.Core.Application.Features.FileSystem;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Core.Application;

public static class Registration
{
    public static void AddCoreServices(this IServiceCollection services)
    {
        services.AddFileSystemFeature();
    }
}
