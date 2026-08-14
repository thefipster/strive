using Fip.Strive.Application.Features.Catalog.Services;
using Fip.Strive.Application.Features.Catalog.Services.Contracts;
using Fip.Strive.Application.Features.Import.Services;
using Fip.Strive.Application.Features.Import.Services.Contracts;
using Fip.Strive.Application.Features.Jobs;
using Fip.Strive.Application.Features.Jobs.Services;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Fip.Strive.Application.Features.Storage;
using Fip.Strive.Application.Features.Storage.Services;
using Fip.Strive.Application.Features.Storage.Services.Contracts;
using Fip.Strive.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Application;

/// <summary>
/// Composition root for the application layer.
/// </summary>
public static class Registration
{
    /// <param name="basePath">
    /// What a relative <see cref="StorageOptions.DataDirectory"/> resolves against — the host's
    /// content root in the app, a temp directory in tests.
    /// </param>
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration,
        string basePath
    )
    {
        services.AddPersistence(configuration);
        services.AddStorage(configuration, basePath);
        services.AddCatalog();
        services.AddJobs(configuration);

        services.TryAddTimeProvider();

        return services;
    }

    private static void AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString = configuration.GetConnectionString(DatabaseName);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                $"Connection string '{DatabaseName}' is missing. In development the Aspire AppHost "
                    + "supplies it; in a container set ConnectionStrings__strive."
            );

        services.AddDbContext<StriveContext>(options => options.UseNpgsql(connectionString));
    }

    private static void AddStorage(
        this IServiceCollection services,
        IConfiguration configuration,
        string basePath
    )
    {
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));

        services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<StorageOptions>>().Value;

            // GetFullPath ignores basePath when DataDirectory is already absolute, which is
            // exactly the relative-or-absolute contract the option documents.
            var paths = new StoragePaths(Path.GetFullPath(options.DataDirectory, basePath));
            paths.EnsureCreated();
            return paths;
        });

        services.AddSingleton<IBlobStore, BlobStore>();
        services.AddSingleton<IStagingArea, StagingArea>();
    }

    private static void AddCatalog(this IServiceCollection services)
    {
        services.AddScoped<ICatalogReader, CatalogReader>();
        services.AddScoped<IPackageImporter, PackageImporter>();
    }

    private static void AddJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JobOptions>(configuration.GetSection(JobOptions.SectionName));

        // The signal and the notifier are the two things every scope shares.
        services.AddSingleton<JobSignal>();
        services.AddSingleton<IJobNotifier, JobNotifier>();

        // Built once from a throwaway scope. It keeps only each handler's identity, never the
        // handler, so nothing outlives the scope it was resolved from.
        services.AddSingleton<IJobRegistry>(provider =>
        {
            using var scope = provider.CreateScope();
            return new JobRegistry(scope.ServiceProvider.GetServices<IJobHandler>());
        });

        services.AddScoped<IJobQueue, JobQueue>();
        services.AddScoped<IJobStore, JobStore>();

        services.AddHostedService<JobRunner>();
    }

    private static void TryAddTimeProvider(this IServiceCollection services)
    {
        if (services.All(descriptor => descriptor.ServiceType != typeof(TimeProvider)))
            services.AddSingleton(TimeProvider.System);
    }

    /// <summary>Connection string name, shared with the Aspire AppHost's database resource.</summary>
    public const string DatabaseName = "strive";
}
