using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Harvester.Application.Core.Indexing.Repositories;
using Fip.Strive.Harvester.Application.Core.Indexing.Repositories.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Component;
using Fip.Strive.Harvester.Application.Features.Expand.Component.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Component.Decorators;
using Fip.Strive.Harvester.Application.Features.Expand.Repositories;
using Fip.Strive.Harvester.Application.Features.Expand.Services;
using Fip.Strive.Harvester.Application.Features.Expand.Services.Contracts;
using Fip.Strive.Harvester.Application.Infrastructure.Repositories.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Features.Expand;

public static class Registration
{
    public static void AddExpandFeature(this IServiceCollection services)
    {
        services.AddScoped<IZipExtractor, UnzipCoordinator>();

        services.AddScoped<IZipEnumerator, ZipEnumerator>();

        services.AddScoped<IFileHashGate, FileHashGate>();
        services.Decorate<IFileHashGate, FileHashGateSignalled>();

        services.AddScoped<IIndexer<FileIndex, string>, FileIndexer>();
        services.AddScoped<ISpecificationReader<FileIndex>, FilePager>();

        services.AddScoped<IExpansionService, ExpansionService>();
    }
}
