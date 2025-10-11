using FluentValidation;
using TheFipster.ActivityAggregator.Api.Extensions;
using TheFipster.ActivityAggregator.Api.Interceptors;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Importer;
using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;
using TheFipster.ActivityAggregator.Storage.Lite.Components.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Scan;

public static class ServiceExtension
{
    public static void AddScannerFeature(this IServiceCollection services)
    {
        services.AddImporterInfoFeature();
        services.AddScanningFeature();
        services.AddScanPageFeature();
    }

    private static void AddImporterInfoFeature(this IServiceCollection services)
    {
        services.AddSingleton<IImporterRegistry, Registry>();

        services.AddScoped<IClassifiersAction, ClassifiersAction>();

        services.AddScoped<IClassifier, Classifier>();
    }

    private static void AddScanningFeature(this IServiceCollection services)
    {
        services.AddScoped<IIndexer<FileIndex>, BaseIndexer<FileIndex>>();

        services.AddScoped<IScanAction, ScanAction>();

        services.AddInterceptedScoped<IScannerService, ScannerService>(typeof(TracingInterceptor));

        services.AddInterceptedScoped<IFileScanner, FileScanner>(typeof(TracingInterceptor));
        services.Decorate<IFileScanner, FileScannerIndexer>();
    }

    private static void AddScanPageFeature(this IServiceCollection services)
    {
        services.AddScoped<IPagedIndexer<FileIndex>, PagedIndexer<FileIndex>>();

        services.AddScoped<IFilesAction, FilesAction>();
        services.AddScoped<IValidator<ScanFilePageRequest>, ScanFilePageRequestValidator>();
        services.Decorate<IFilesAction, FilesActionValidator>();
    }
}
