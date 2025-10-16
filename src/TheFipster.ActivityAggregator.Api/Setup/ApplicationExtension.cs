using TheFipster.ActivityAggregator.Api.Features.Assimilate;
using TheFipster.ActivityAggregator.Api.Features.Batch;
using TheFipster.ActivityAggregator.Api.Features.Core;
using TheFipster.ActivityAggregator.Api.Features.Scan;
using TheFipster.ActivityAggregator.Api.Features.Upload;
using TheFipster.ActivityAggregator.Importer;
using TheFipster.ActivityAggregator.Storage.Lite;

namespace TheFipster.ActivityAggregator.Api.Setup;

public static class ApplicationExtension
{
    public static void AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddConfig(configuration);
        services.AddImporters();
        services.AddLiteDbStorage(configuration);
        services.AddCoreFeature();
        services.AddUploadFeature();
        services.AddScannerFeature();
        services.AddAssimilateFeature();
        services.AddBatchFeature();
    }
}
