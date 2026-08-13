using Fip.Strive.Tracking.Application.Features.Access;
using Fip.Strive.Tracking.Application.Features.Access.Services;
using Fip.Strive.Tracking.Application.Features.Access.Services.Contracts;
using Fip.Strive.Tracking.Application.Features.Events.Services;
using Fip.Strive.Tracking.Application.Features.Events.Services.Contracts;
using Fip.Strive.Tracking.Application.Features.Export.Services;
using Fip.Strive.Tracking.Application.Features.Export.Services.Contracts;
using Fip.Strive.Tracking.Application.Features.Storage;
using Fip.Strive.Tracking.Application.Features.Trackers.Services;
using Fip.Strive.Tracking.Application.Features.Trackers.Services.Contracts;
using Fip.Strive.Tracking.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Tracking.Application;

/// <summary>
/// Composition root for the tracking application layer.
/// </summary>
public static class Registration
{
    /// <param name="basePath">
    /// What a relative <see cref="TrackingStorageOptions.DataDirectory"/> resolves against — the
    /// host's content root in the app, a temp directory in tests.
    /// </param>
    public static IServiceCollection AddTracking(
        this IServiceCollection services,
        IConfiguration configuration,
        string basePath
    )
    {
        services.AddPersistence(configuration, basePath);
        services.AddTrackers();
        services.AddAccess(configuration);

        services.TryAddTimeProvider();

        return services;
    }

    private static void AddAccess(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(AccessOptions.SectionName);
        var options = section.Get<AccessOptions>() ?? new AccessOptions();

        // Refused at startup rather than at the first login attempt: this app is meant to sit on the
        // open internet, and an instance nobody can log into is far better than one anybody can.
        if (!Pbkdf2Password.IsEncodedHash(options.PasswordHash))
            throw new InvalidOperationException(
                $"'{AccessOptions.SectionName}:PasswordHash' is missing or not a PBKDF2 hash. "
                    + "Generate one with `dotnet run --project src/Fip.Strive.Tracking.Web -- "
                    + "hash-password` and supply it as Access__PasswordHash."
            );

        if (
            !string.IsNullOrEmpty(options.ApiKey)
            && options.ApiKey.Length < AccessOptions.MinimumApiKeyLength
        )
            throw new InvalidOperationException(
                $"'{AccessOptions.SectionName}:ApiKey' is shorter than "
                    + $"{AccessOptions.MinimumApiKeyLength} characters. Leave it empty to turn the "
                    + "API off, or set a generated secret."
            );

        services.Configure<AccessOptions>(section);
        services.AddSingleton<IAccessGuard, AccessGuard>();
    }

    private static void AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        string basePath
    )
    {
        services.Configure<TrackingStorageOptions>(
            configuration.GetSection(TrackingStorageOptions.SectionName)
        );

        services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<TrackingStorageOptions>>().Value;

            // GetFullPath ignores basePath when DataDirectory is already absolute, which is exactly
            // the relative-or-absolute contract the option documents.
            var database = new TrackingDatabase(
                Path.GetFullPath(options.DataDirectory, basePath),
                options.DatabaseFileName
            );

            database.EnsureDirectoryCreated();
            return database;
        });

        services.AddDbContext<TrackingContext>(
            (provider, builder) =>
                builder.UseSqlite(provider.GetRequiredService<TrackingDatabase>().ConnectionString)
        );
    }

    private static void AddTrackers(this IServiceCollection services)
    {
        services.AddScoped<ITrackerReader, TrackerReader>();
        services.AddScoped<ITrackerWriter, TrackerWriter>();
        services.AddScoped<IEventReader, EventReader>();
        services.AddScoped<IEventRecorder, EventRecorder>();
        services.AddScoped<IExportReader, ExportReader>();
    }

    private static void TryAddTimeProvider(this IServiceCollection services)
    {
        if (services.All(descriptor => descriptor.ServiceType != typeof(TimeProvider)))
            services.AddSingleton(TimeProvider.System);
    }
}
