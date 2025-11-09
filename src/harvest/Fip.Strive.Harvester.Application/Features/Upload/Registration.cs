using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Harvester.Application.Features.Upload.Services;
using Fip.Strive.Harvester.Application.Features.Upload.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Upload.Services.Decorators;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Features.Upload;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static void AddUploadFeature(this IServiceCollection services)
    {
        services.AddScoped<IUploadService, UploadService>();
        services.Decorate<IUploadService, UploadValidation>();
        services.Decorate<IUploadService, UploadEmission>();
    }
}
