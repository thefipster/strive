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
        services.AddLiteDbStorage(configuration);
        services.AddInfoFeature();
        services.AddQueueFeature();
        services.AddImporterFeature();
        services.AddCoreFeature();
        services.AddUploadFeature();
        services.AddScannerFeature();
        services.AddAssimilateFeature();
        services.AddBatchFeature();
    }
}
