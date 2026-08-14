using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Fip.Strive.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.IntegrationTests.Fixtures;

/// <summary>
/// A private migrated database plus the job services, registered the way the app registers them —
/// so the tests exercise the real composition rather than a hand-wired variant of it.
/// </summary>
public sealed class JobHarness : IAsyncDisposable
{
    private readonly string _connectionString;
    private readonly ServiceProvider _provider;

    private JobHarness(string connectionString, Action<IServiceCollection>? configure)
    {
        _connectionString = connectionString;

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddOptions();
        services.AddSingleton(TimeProvider.System);
        services.AddDbContext<StriveContext>(builder => builder.UseNpgsql(connectionString));

        services.AddSingleton<JobSignal>();
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddScoped<IJobStore, JobStore>();

        // Handlers and anything they need are registered by the caller, exactly as a feature
        // registers its own handler in Registration.
        configure?.Invoke(services);

        services.AddSingleton<IJobRegistry>(provider =>
        {
            using var scope = provider.CreateScope();
            return new JobRegistry(scope.ServiceProvider.GetServices<IJobHandler>());
        });

        _provider = services.BuildServiceProvider();
    }

    /// <remarks>
    /// No default on <paramref name="configure"/>: with one, a bare <c>CreateAsync(fixture)</c>
    /// would match this and the params overload equally and fail to compile.
    /// </remarks>
    public static async Task<JobHarness> CreateAsync(
        PostgresFixture fixture,
        Action<IServiceCollection> configure
    )
    {
        var harness = new JobHarness(await fixture.CreateDatabaseAsync(), configure);

        await using var context = harness.CreateContext();
        await context.Database.MigrateAsync();

        return harness;
    }

    /// <summary>Registers stateless stub handlers, which is all most job tests need.</summary>
    public static Task<JobHarness> CreateAsync(
        PostgresFixture fixture,
        params IJobHandler[] handlers
    ) =>
        CreateAsync(
            fixture,
            services =>
            {
                foreach (var handler in handlers)
                    services.AddScoped(_ => handler);
            }
        );

    /// <summary>A context outside the provider, so assertions read the database, not a tracker.</summary>
    public StriveContext CreateContext() =>
        new(new DbContextOptionsBuilder<StriveContext>().UseNpgsql(_connectionString).Options);

    public AsyncServiceScope Scope() => _provider.CreateAsyncScope();

    public T Resolve<T>()
        where T : notnull => _provider.GetRequiredService<T>();

    public async Task<Guid> EnqueueAsync(string kind, string targetKey, object? payload = null)
    {
        await using var scope = Scope();
        return await scope
            .ServiceProvider.GetRequiredService<IJobQueue>()
            .EnqueueAsync(kind, targetKey, payload);
    }

    /// <summary>Writes a job row directly, so a test can start from any state.</summary>
    public async Task<Guid> SeedAsync(
        string kind,
        string targetKey,
        JobState state,
        int attempts = 0,
        DateTimeOffset? enqueued = null
    )
    {
        var job = new Job
        {
            Id = Guid.CreateVersion7(),
            Kind = kind,
            TargetKey = targetKey,
            ComponentId = kind,
            ComponentVersion = 1,
            State = state,
            Attempts = attempts,
            EnqueuedUtc = enqueued ?? DateTimeOffset.UtcNow,
            StartedUtc = state == JobState.Running ? DateTimeOffset.UtcNow : null,
        };

        await using var context = CreateContext();
        context.Jobs.Add(job);
        await context.SaveChangesAsync();

        return job.Id;
    }

    public async ValueTask DisposeAsync()
    {
        await using (var context = CreateContext())
        {
            await context.Database.EnsureDeletedAsync();
        }

        await _provider.DisposeAsync();
    }
}
