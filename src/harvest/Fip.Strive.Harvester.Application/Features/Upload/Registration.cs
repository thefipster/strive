using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Harvester.Application.Features.Upload.Services;
using Fip.Strive.Harvester.Application.Features.Upload.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Upload.Services.Decorators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Features.Upload;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static void AddUploadFeature(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddTransient<IUploadService, UploadService>();
        services.Decorate<IUploadService, UploadServiceValidator>();
        services.Decorate<IUploadService, UploadServiceSignaller>();
    }
}
