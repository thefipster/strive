using FluentValidation;
using TheFipster.ActivityAggregator.Api.Extensions;
using TheFipster.ActivityAggregator.Api.Interceptors;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;
using TheFipster.ActivityAggregator.Storage.Lite.Components.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Upload;

public static class ServiceExtension
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
        services.AddScoped<IValidator<UploadChunkRequest>, UploadChunkValidator>();
        services.Decorate<IChunkAction, ChunkActionValidator>();

        services.AddScoped<IUnzipService, UnzipService>();
        services.Decorate<IUnzipService, UnzipIndexer>();
        services.Decorate<IUnzipService, UnzipNotifier>();

        services.AddInterceptedScoped<IUnzipper, Unzipper>(typeof(TracingInterceptor));
        services.Decorate<IUnzipper, UnzipperValidator>();
    }

    private static void AddZipPagingFeature(this IServiceCollection services)
    {
        services.AddScoped<IPagedIndexer<ZipIndex>, PagedIndexer<ZipIndex>>();

        services.AddScoped<IZipsAction, ZipsAction>();
        services.AddScoped<IValidator<ZipsPageRequest>, ZipsPageValidator>();
        services.Decorate<IZipsAction, ZipsActionValidator>();
    }
}
