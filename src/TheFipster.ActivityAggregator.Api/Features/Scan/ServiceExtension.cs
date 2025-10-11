using FluentValidation;
using TheFipster.ActivityAggregator.Api.Extensions;
using TheFipster.ActivityAggregator.Api.Features.Scan.Services.Decorators;
using TheFipster.ActivityAggregator.Api.Interceptors;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
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
        services.AddScoped<IClassifiersAction, ClassifiersAction>();
    }

    private static void AddScanningFeature(this IServiceCollection services)
    {
        services.AddScoped<IScanAction, ScanAction>();

        services.AddInterceptedScoped<IScannerService, ScannerService>(typeof(TracingInterceptor));
        services.Decorate<IScannerService, ScannerNotifier>();

        services.AddInterceptedScoped<IFileScanner, FileScanner>(typeof(TracingInterceptor));
        services.Decorate<IFileScanner, FileScannerIndexer>();

        services.AddScoped<IIndexer<FileIndex>, BaseIndexer<FileIndex>>();

        services.AddScoped<IClassifier, Classifier>();
    }

    private static void AddScanPageFeature(this IServiceCollection services)
    {
        services.AddScoped<IPagedIndexer<FileIndex>, PagedIndexer<FileIndex>>();

        services.AddScoped<IFilePageAction, FilePageAction>();
        services.AddScoped<IValidator<ScanFilePageRequest>, ScanFilePageRequestValidator>();
        services.Decorate<IFilePageAction, FilesActionValidator>();
    }
}
