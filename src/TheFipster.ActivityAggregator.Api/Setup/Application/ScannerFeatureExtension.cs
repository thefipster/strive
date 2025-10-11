using TheFipster.ActivityAggregator.Api.Extensions;
using TheFipster.ActivityAggregator.Api.Features.Scan.Components;
using TheFipster.ActivityAggregator.Api.Features.Scan.Components.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Scan.Components.Decorators;
using TheFipster.ActivityAggregator.Api.Features.Scan.Mediators;
using TheFipster.ActivityAggregator.Api.Features.Scan.Mediators.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Scan.Services;
using TheFipster.ActivityAggregator.Api.Features.Scan.Services.Contracts;
using TheFipster.ActivityAggregator.Api.Interceptors;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Importer;
using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;
using TheFipster.ActivityAggregator.Storage.Lite.Components.Indexer;

namespace TheFipster.ActivityAggregator.Api.Setup.Application;

public static class ScannerFeatureExtension
{
    public static void AddScannerFeature(this IServiceCollection services)
    {
        services.AddSingleton<IImporterRegistry, Registry>();
        services.AddScoped<IClassifiersAction, ClassifiersAction>();

        services.AddScoped<IIndexer<FileIndex>, BaseIndexer<FileIndex>>();
        services.AddScoped<IClassifier, Classifier>();
        services.AddInterceptedScoped<IFileScanner, FileScanner>(typeof(TracingInterceptor));
        services.Decorate<IFileScanner, FileScannerIndexer>();
        services.AddInterceptedScoped<IScannerService, ScannerService>(typeof(TracingInterceptor));
        services.AddScoped<IScanAction, ScanAction>();

        services.AddScoped<IPagedIndexer<FileIndex>, PagedIndexer<FileIndex>>();
        services.AddScoped<IFilesAction, FilesAction>();
    }
}
