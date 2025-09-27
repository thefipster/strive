using TheFipster.ActivityAggregator.Api.Abtraction;
using TheFipster.ActivityAggregator.Api.Components;
using TheFipster.ActivityAggregator.Api.Services;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Services.Abstractions;
using TheFipster.ActivityAggregator.Services.Components;
using TheFipster.ActivityAggregator.Services.Worker;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;
using TheFipster.ActivityAggregator.Storage.Lite.Components.Indexer;

namespace TheFipster.ActivityAggregator.Api;

public static class Setup
{
    public static IServiceCollection AddCustom(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<ApiConfig>(configuration.GetSection(ApiConfig.ConfigSectionName));

        services.AddTransient<IUploader, Uploader>();

        services.AddTransient<IExtractor, Extractor>();
        services.AddTransient<IUnzipper, Unzipper>();
        services.AddTransient<IIndexer<ImporterIndex>, BaseIndexer<ImporterIndex>>();

        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<QueuedHostedService>();

        return services;
    }

    public static IServiceCollection AddCorsPolicies(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowAll",
                policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
            );
        });

        services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowOne",
                policy =>
                {
                    policy.WithOrigins("https://localhost:7260").AllowAnyMethod().AllowAnyHeader();
                }
            );
        });

        return services;
    }
}
