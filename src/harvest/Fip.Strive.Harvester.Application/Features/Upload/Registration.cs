using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Features.Importer.Worker;
using Fip.Strive.Harvester.Application.Features.Upload.Services;
using Fip.Strive.Harvester.Application.Features.Upload.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Upload.Services.Decorators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Features.Upload;

public static class Registration
{
    public static void AddUploadFeature(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<UploadConfig>(configuration.GetSection(UploadConfig.ConfigSectionName));
        services.AddTransient<IUploadService, UploadService>();
        services.Decorate<IUploadService, UploadServiceValidator>();
    }
}
