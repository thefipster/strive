using Fip.Strive.Core.Application.Features.FileSystem.Services;
using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Core.Application.Features.FileSystem;

public static class Registration
{
    public static void AddFileSystemFeature(this IServiceCollection services)
    {
        services.AddScoped<IDirectoryService, DirectoryService>();

        services.AddScoped<IZipExtractor, ZipFileExtractor>();
    }
}
