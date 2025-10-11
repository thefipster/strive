using TheFipster.ActivityAggregator.Api.Extensions;
using TheFipster.ActivityAggregator.Api.Features.Upload.Components;
using TheFipster.ActivityAggregator.Api.Features.Upload.Components.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Upload.Components.Decorators;
using TheFipster.ActivityAggregator.Api.Features.Upload.Mediators;
using TheFipster.ActivityAggregator.Api.Features.Upload.Mediators.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Upload.Mediators.Decorators;
using TheFipster.ActivityAggregator.Api.Features.Upload.Services;
using TheFipster.ActivityAggregator.Api.Features.Upload.Services.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Upload.Services.Decorators;
using TheFipster.ActivityAggregator.Api.Interceptors;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;
using TheFipster.ActivityAggregator.Storage.Lite.Components.Indexer;

namespace TheFipster.ActivityAggregator.Api.Setup.Application;

public static class UploadFeatureExtension
{
    public static void AddUploadFeature(this IServiceCollection services)
    {
        services.AddZipUploadFeature();
        services.AddZipPagingFeature();
    }

    private static void AddZipUploadFeature(this IServiceCollection services)
    {
        services.AddScoped<IIndexer<ZipIndex>, BaseIndexer<ZipIndex>>();

        services.AddScoped<IUploader, Uploader>();

        services.AddScoped<IChunkAction, ChunkAction>();
        services.Decorate<IChunkAction, ChunkActionValidator>();

        services.AddInterceptedScoped<IUnzipper, Unzipper>(typeof(TracingInterceptor));
        services.Decorate<IUnzipper, UnzipperValidator>();

        services.AddInterceptedScoped<IUnzipService, UnzipService>(typeof(TracingInterceptor));
        services.Decorate<IUnzipService, UnzipIndexer>();
        services.Decorate<IUnzipService, UnzipNotifier>();
    }

    private static void AddZipPagingFeature(this IServiceCollection services)
    {
        services.AddScoped<IPagedIndexer<ZipIndex>, PagedIndexer<ZipIndex>>();

        services.AddScoped<IZipsAction, ZipsAction>();
        services.Decorate<IZipsAction, ZipsActionValidator>();
    }
}
