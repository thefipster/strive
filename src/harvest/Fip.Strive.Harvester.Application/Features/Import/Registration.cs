using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Harvester.Application.Features.Import.Services;
using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Services.Decorators;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Features.Import;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static void AddImportFeature(this IServiceCollection services)
    {
        services.AddScoped<IZipFileAccess, ZipFileAccess>();
        services.Decorate<IZipFileAccess, ZipFileAccessValidator>();

        services.AddScoped<IImportService, ImportService>();
        services.Decorate<IImportService, ImportServiceSignaller>();
    }
}
