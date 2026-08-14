# Job Engine Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a durable job table, an in-process executor with startup recovery, and a live jobs page, then move archive unpacking onto it.

**Architecture:** Postgres is the queue. `IJobQueue` upserts a `Pending` row keyed by `(kind, target_key)`; a pump inside `JobRunner : BackgroundService` claims batches with `SELECT … FOR UPDATE SKIP LOCKED` and feeds a bounded `Channel<Guid>`; N workers each open a DI scope and execute the handler registered for the job's kind. The UI is a read-only view of the table, refreshed on a signal.

**Tech Stack:** .NET 10, EF Core 10.0.11 + Npgsql 10.0.3, Blazor Server + MudBlazor 9.8.0, xunit 2.9.3 + AwesomeAssertions 9.5.0, Testcontainers 4.13.0.

**Spec:** [docs/superpowers/specs/2026-08-14-job-engine-design.md](../specs/2026-08-14-job-engine-design.md)

## Global Constraints

- **Formatting is enforced.** Run `dotnet csharpier format src/` before every commit. CI rejects anything CSharpier would reformat.
- **Central package management.** Never put a `Version=` on a `PackageReference`. Versions live in `src/Directory.Packages.props`.
- **Nullable and implicit usings are on** solution-wide (`src/Directory.Build.props`). Do not add `using System;` and friends.
- **Analyzers run as errors in CI** (`-warnaserror`). Local builds warn.
- **Naming:** tables and columns are `snake_case`; C# is standard PascalCase. Test method names use `Underscores_between_words` and read as sentences.
- **Timestamps** are `DateTimeOffset` stored as `timestamptz`, always UTC, always from an injected `TimeProvider` — never `DateTimeOffset.UtcNow` in application code. Test code may use it freely.
- **Ids** are `Guid.CreateVersion7()`.
- **`JobState` persists as its enum name** (`Pending`, `Running`, `Succeeded`, `Failed`, `Stale`) via `HasConversion<string>()`, never as an ordinal.
- **Integration tests need Docker.** `dotnet test src/Fip.Strive.IntegrationTests` fails rather than skips without it.
- **Comments explain why, not what.** Match the density of the surrounding code — see `PackageImporter` for the house style.

---

### Task 1: Job entity, configuration, migration

**Files:**
- Create: `src/Fip.Strive.Application/Features/Jobs/Models/JobState.cs`
- Create: `src/Fip.Strive.Application/Features/Jobs/Models/Job.cs`
- Create: `src/Fip.Strive.Application/Infrastructure/Configurations/JobConfiguration.cs`
- Modify: `src/Fip.Strive.Application/Infrastructure/StriveContext.cs`
- Test: `src/Fip.Strive.IntegrationTests/JobSchemaTests.cs`

**Interfaces:**
- Consumes: nothing.
- Produces: `JobState` enum; `Job` entity with `Id`, `Kind`, `TargetKey`, `ComponentId`, `ComponentVersion`, `State`, `Attempts`, `Payload`, `Error`, `ProgressCurrent`, `ProgressTotal`, `ProgressNote`, `EnqueuedUtc`, `StartedUtc`, `FinishedUtc`; `StriveContext.Jobs`.

- [ ] **Step 1: Write the failing test**

Create `src/Fip.Strive.IntegrationTests/JobSchemaTests.cs`:

```csharp
using AwesomeAssertions;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.IntegrationTests;

[Collection(PostgresCollection.Name)]
public class JobSchemaTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Migrations_produce_an_empty_job_table()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);
        await using var context = harness.CreateContext();

        (await context.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
        (await context.Jobs.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task A_work_unit_can_only_exist_once()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);
        await using var context = harness.CreateContext();

        context.Jobs.Add(NewJob());
        await context.SaveChangesAsync();

        context.Jobs.Add(NewJob());

        // The unique index is what makes an enqueue an upsert rather than an append; without it a
        // replay would grow a row per run.
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task State_is_stored_as_its_name_not_its_ordinal()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        await using (var context = harness.CreateContext())
        {
            context.Jobs.Add(NewJob());
            await context.SaveChangesAsync();
        }

        await using var reader = harness.CreateContext();
        var stored = await reader
            .Database.SqlQueryRaw<string>("SELECT state AS \"Value\" FROM jobs")
            .SingleAsync();

        stored.Should().Be("Pending", "reordering the enum must never reinterpret existing rows");
    }

    private static Job NewJob() =>
        new()
        {
            Id = Guid.CreateVersion7(),
            Kind = "unpack",
            TargetKey = "abc123",
            ComponentId = "unpack",
            ComponentVersion = 1,
            State = JobState.Pending,
            EnqueuedUtc = DateTimeOffset.UtcNow,
        };
}
```

- [ ] **Step 2: Run the test to verify it fails**

```bash
dotnet test src/Fip.Strive.IntegrationTests --filter JobSchemaTests
```

Expected: compile failure — `Job`, `JobState` and `context.Jobs` do not exist.

- [ ] **Step 3: Write the model**

`src/Fip.Strive.Application/Features/Jobs/Models/JobState.cs`:

```csharp
namespace Fip.Strive.Application.Features.Jobs.Models;

public enum JobState
{
    Pending,
    Running,
    Succeeded,
    Failed,

    /// <summary>
    /// The component that produced this unit has moved on. Nothing sets this yet — step 3's
    /// invalidation sweep does — but startup recovery already re-queues it.
    /// </summary>
    Stale,
}
```

`src/Fip.Strive.Application/Features/Jobs/Models/Job.cs`:

```csharp
namespace Fip.Strive.Application.Features.Jobs.Models;

/// <summary>
/// One row per work unit, not per run. Re-running a unit updates this row, which is what keeps the
/// table bounded by how much work there is rather than by how often it has been replayed.
/// </summary>
public class Job
{
    public Guid Id { get; set; }

    public string Kind { get; set; } = string.Empty;

    /// <summary>
    /// The unit's natural key within its kind — an archive hash for unpacking, a catalog hash for
    /// classification later. Unique together with <see cref="Kind"/>.
    /// </summary>
    public string TargetKey { get; set; } = string.Empty;

    public string ComponentId { get; set; } = string.Empty;

    /// <summary>Stamped at enqueue. Step 3 compares it against the registry to find stale units.</summary>
    public int ComponentVersion { get; set; }

    public JobState State { get; set; }

    public int Attempts { get; set; }

    /// <summary>Kind-specific JSON. Opaque to everything but the handler that wrote it.</summary>
    public string? Payload { get; set; }

    public string? Error { get; set; }

    public int? ProgressCurrent { get; set; }

    public int? ProgressTotal { get; set; }

    public string? ProgressNote { get; set; }

    public DateTimeOffset EnqueuedUtc { get; set; }

    public DateTimeOffset? StartedUtc { get; set; }

    public DateTimeOffset? FinishedUtc { get; set; }
}
```

- [ ] **Step 4: Write the configuration**

`src/Fip.Strive.Application/Infrastructure/Configurations/JobConfiguration.cs`:

```csharp
using Fip.Strive.Application.Features.Jobs.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Application.Infrastructure.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("jobs");

        builder.HasKey(job => job.Id);

        builder.Property(job => job.Kind).HasMaxLength(64).IsRequired();

        builder.Property(job => job.TargetKey).HasMaxLength(512).IsRequired();

        builder.Property(job => job.ComponentId).HasMaxLength(128).IsRequired();

        // Stored as the enum's name rather than its ordinal: the column stays readable in psql,
        // and inserting a member in the middle of the enum later cannot silently reinterpret
        // every existing row.
        builder.Property(job => job.State).HasConversion<string>().HasMaxLength(16).IsRequired();

        builder.Property(job => job.Payload).HasColumnType("jsonb");

        builder.Property(job => job.ProgressNote).HasMaxLength(1024);

        // A work unit exists once. Enqueueing a known unit is an upsert back to Pending, which is
        // what the spec means by a unit recording the version that last succeeded.
        builder.HasIndex(job => new { job.Kind, job.TargetKey }).IsUnique();

        // The pump's claim query.
        builder.HasIndex(job => new { job.State, job.EnqueuedUtc });

        // Step 3's invalidation sweep.
        builder.HasIndex(job => new { job.ComponentId, job.ComponentVersion });
    }
}
```

- [ ] **Step 5: Register it on the context**

In `src/Fip.Strive.Application/Infrastructure/StriveContext.cs`, add `using Fip.Strive.Application.Features.Jobs.Models;`, the DbSet after `PackageFiles`:

```csharp
    public DbSet<Job> Jobs => Set<Job>();
```

and the configuration inside `OnModelCreating`, after `PackageFileConfiguration`:

```csharp
        modelBuilder.ApplyConfiguration(new JobConfiguration());
```

- [ ] **Step 6: Generate the migration**

```bash
dotnet ef migrations add Jobs --project src/Fip.Strive.Application --startup-project src/Fip.Strive.Web
```

Open the generated `Up` method and confirm it creates `jobs` with the three indexes and touches no other table.

- [ ] **Step 7: Run the tests to verify they pass**

```bash
dotnet test src/Fip.Strive.IntegrationTests --filter JobSchemaTests
```

Expected: 3 passed.

- [ ] **Step 8: Format and commit**

```bash
dotnet csharpier format src/
git add src/
git commit -m "feat: add the job table"
```

---

### Task 2: Handler contract and component registry

**Files:**
- Create: `src/Fip.Strive.Application/Features/Jobs/Models/JobProgress.cs`
- Create: `src/Fip.Strive.Application/Features/Jobs/Models/JobContext.cs`
- Create: `src/Fip.Strive.Application/Features/Jobs/Models/JobComponent.cs`
- Create: `src/Fip.Strive.Application/Features/Jobs/Services/Contracts/IJobHandler.cs`
- Create: `src/Fip.Strive.Application/Features/Jobs/Services/Contracts/IJobRegistry.cs`
- Create: `src/Fip.Strive.Application/Features/Jobs/Services/JobRegistry.cs`
- Test: `src/Fip.Strive.Application.UnitTests/Jobs/JobRegistryTests.cs`

**Interfaces:**
- Consumes: `Job` (Task 1).
- Produces: `IJobHandler` with `string Kind`, `string ComponentId`, `int Version`, `Task ExecuteAsync(JobContext, CancellationToken)`; `JobComponent(string Kind, string ComponentId, int Version)`; `IJobRegistry` with `JobComponent Resolve(string kind)` and `IReadOnlyCollection<JobComponent> All { get; }`; `JobContext(Job Job, IProgress<JobProgress> Progress)`; `JobProgress(int Current, int Total, string? Note)`.

The registry deliberately holds **metadata, not handler instances**. Handlers are scoped (they depend on scoped services like `IPackageImporter`), and a singleton registry retaining instances from a startup scope would be holding objects whose `DbContext` has been disposed. The runner resolves the instance it executes from the job's own scope.

- [ ] **Step 1: Write the failing test**

Create `src/Fip.Strive.Application.UnitTests/Jobs/JobRegistryTests.cs`:

```csharp
using AwesomeAssertions;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;

namespace Fip.Strive.Application.UnitTests.Jobs;

public class JobRegistryTests
{
    [Fact]
    public void Resolves_a_components_identity_by_its_kind()
    {
        var registry = new JobRegistry([new StubHandler("unpack", "unpack", 3)]);

        var component = registry.Resolve("unpack");

        component.ComponentId.Should().Be("unpack");
        component.Version.Should().Be(3);
    }

    [Fact]
    public void An_unknown_kind_is_an_error_rather_than_a_silent_no_op()
    {
        var registry = new JobRegistry([new StubHandler("unpack", "unpack", 1)]);

        var act = () => registry.Resolve("classify");

        act.Should().Throw<InvalidOperationException>().WithMessage("*classify*");
    }

    [Fact]
    public void Two_handlers_claiming_one_kind_fail_at_construction()
    {
        // Picking one arbitrarily would mean the handler that ran was decided by DI registration
        // order, which is invisible at the point anything goes wrong.
        var act = () =>
            new JobRegistry([new StubHandler("unpack", "a", 1), new StubHandler("unpack", "b", 1)]);

        act.Should().Throw<InvalidOperationException>().WithMessage("*unpack*");
    }

    [Fact]
    public void Every_registered_component_is_listed()
    {
        var registry = new JobRegistry(
            [new StubHandler("unpack", "unpack", 1), new StubHandler("classify", "classify", 4)]
        );

        registry
            .All.Select(component => (component.ComponentId, component.Version))
            .Should()
            .BeEquivalentTo([("unpack", 1), ("classify", 4)]);
    }

    private sealed class StubHandler(string kind, string componentId, int version) : IJobHandler
    {
        public string Kind => kind;

        public string ComponentId => componentId;

        public int Version => version;

        public Task ExecuteAsync(JobContext context, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
```

- [ ] **Step 2: Run the test to verify it fails**

```bash
dotnet test src/Fip.Strive.Application.UnitTests --filter JobRegistryTests
```

Expected: compile failure — `IJobHandler`, `JobContext`, `JobRegistry` do not exist.

- [ ] **Step 3: Write the models**

`src/Fip.Strive.Application/Features/Jobs/Models/JobProgress.cs`:

```csharp
namespace Fip.Strive.Application.Features.Jobs.Models;

/// <param name="Note">What is being worked on right now — a file path, for unpacking.</param>
public readonly record struct JobProgress(int Current, int Total, string? Note = null);
```

`src/Fip.Strive.Application/Features/Jobs/Models/JobContext.cs`:

```csharp
namespace Fip.Strive.Application.Features.Jobs.Models;

/// <summary>
/// Everything a handler is given. <paramref name="Progress"/> is throttled on the way to the
/// database, so a handler may report as often as is natural for it.
/// </summary>
public sealed record JobContext(Job Job, IProgress<JobProgress> Progress);
```

`src/Fip.Strive.Application/Features/Jobs/Models/JobComponent.cs`:

```csharp
namespace Fip.Strive.Application.Features.Jobs.Models;

/// <summary>
/// A handler's identity, without the handler. Held by the registry so a singleton never retains
/// a scoped handler — and so step 3's invalidation sweep has something to compare stored versions
/// against without resolving anything.
/// </summary>
public sealed record JobComponent(string Kind, string ComponentId, int Version);
```

- [ ] **Step 4: Write the contracts**

`src/Fip.Strive.Application/Features/Jobs/Services/Contracts/IJobHandler.cs`:

```csharp
using Fip.Strive.Application.Features.Jobs.Models;

namespace Fip.Strive.Application.Features.Jobs.Services.Contracts;

/// <summary>
/// One kind of work. Implementations are discovered from DI and must be idempotent: startup
/// recovery re-runs anything that was interrupted, so a handler is expected to survive being run
/// twice against the same target.
/// </summary>
public interface IJobHandler
{
    /// <summary>Matches <see cref="Models.Job.Kind"/>. Unique across all handlers.</summary>
    string Kind { get; }

    /// <summary>
    /// The versioned component this handler's work belongs to. Distinct from <see cref="Kind"/>
    /// because later steps run many versioned components under one kind.
    /// </summary>
    string ComponentId { get; }

    /// <summary>Bumping this is what will mark existing units stale, from step 3 onwards.</summary>
    int Version { get; }

    Task ExecuteAsync(JobContext context, CancellationToken cancellationToken);
}
```

`src/Fip.Strive.Application/Features/Jobs/Services/Contracts/IJobRegistry.cs`:

```csharp
using Fip.Strive.Application.Features.Jobs.Models;

namespace Fip.Strive.Application.Features.Jobs.Services.Contracts;

public interface IJobRegistry
{
    JobComponent Resolve(string kind);

    /// <summary>
    /// Every registered component. Step 3's invalidation sweep reads this to compare declared
    /// versions against the ones stamped on existing rows.
    /// </summary>
    IReadOnlyCollection<JobComponent> All { get; }
}
```

- [ ] **Step 5: Write the registry**

`src/Fip.Strive.Application/Features/Jobs/Services/JobRegistry.cs`:

```csharp
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;

namespace Fip.Strive.Application.Features.Jobs.Services;

public sealed class JobRegistry : IJobRegistry
{
    private readonly Dictionary<string, JobComponent> _byKind;

    public JobRegistry(IEnumerable<IJobHandler> handlers)
    {
        _byKind = new Dictionary<string, JobComponent>(StringComparer.Ordinal);

        foreach (var handler in handlers)
        {
            var component = new JobComponent(handler.Kind, handler.ComponentId, handler.Version);

            // Thrown at construction, which means at startup, rather than the first time a job of
            // the ambiguous kind happens to be claimed.
            if (!_byKind.TryAdd(handler.Kind, component))
                throw new InvalidOperationException(
                    $"Two job handlers claim the kind '{handler.Kind}': "
                        + $"{_byKind[handler.Kind].ComponentId} and {handler.ComponentId}."
                );
        }
    }

    public IReadOnlyCollection<JobComponent> All => _byKind.Values;

    public JobComponent Resolve(string kind) =>
        _byKind.TryGetValue(kind, out var component)
            ? component
            : throw new InvalidOperationException(
                $"No job handler is registered for the kind '{kind}'. A row of this kind exists, "
                    + "so either its handler was removed or its registration is missing."
            );
}
```

- [ ] **Step 6: Run the tests to verify they pass**

```bash
dotnet test src/Fip.Strive.Application.UnitTests --filter JobRegistryTests
```

Expected: 4 passed.

- [ ] **Step 7: Format and commit**

```bash
dotnet csharpier format src/
git add src/
git commit -m "feat: add the job handler contract and component registry"
```

---

### Task 3: Enqueue

**Files:**
- Create: `src/Fip.Strive.Application/Features/Jobs/Services/Contracts/IJobQueue.cs`
- Create: `src/Fip.Strive.Application/Features/Jobs/Services/JobQueue.cs`
- Create: `src/Fip.Strive.Application/Features/Jobs/Services/JobSignal.cs`
- Create: `src/Fip.Strive.IntegrationTests/Fixtures/JobHarness.cs`
- Test: `src/Fip.Strive.IntegrationTests/JobQueueTests.cs`

**Interfaces:**
- Consumes: `Job`, `JobState` (Task 1); `IJobRegistry` (Task 2).
- Produces: `IJobQueue.EnqueueAsync(string kind, string targetKey, object? payload = null, CancellationToken = default)` returning `Guid`; `JobSignal` with `void Set()` and `Task WaitAsync(TimeSpan, CancellationToken)`; `JobHarness` with `CreateContext()`, `Scope()`, `EnqueueAsync(...)`.

- [ ] **Step 1: Write the harness**

Create `src/Fip.Strive.IntegrationTests/Fixtures/JobHarness.cs`:

```csharp
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

    public async ValueTask DisposeAsync()
    {
        await using (var context = CreateContext())
        {
            await context.Database.EnsureDeletedAsync();
        }

        await _provider.DisposeAsync();
    }
}
```

The `params IJobHandler[]` overload registers each instance with `AddScoped(_ => handler)`. That is safe only because the stubs are stateless; the real handler is registered as a type in Task 7 so each scope gets its own.

- [ ] **Step 2: Write the failing test**

Create `src/Fip.Strive.IntegrationTests/JobQueueTests.cs`:

```csharp
using AwesomeAssertions;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Fip.Strive.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.IntegrationTests;

[Collection(PostgresCollection.Name)]
public class JobQueueTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Enqueueing_writes_a_pending_row_stamped_with_the_component()
    {
        await using var harness = await JobHarness.CreateAsync(postgres, new NoopHandler(3));

        await harness.EnqueueAsync("noop", "target-1");

        await using var reader = harness.CreateContext();
        var job = await reader.Jobs.SingleAsync();

        job.State.Should().Be(JobState.Pending);
        job.Kind.Should().Be("noop");
        job.TargetKey.Should().Be("target-1");
        job.ComponentId.Should().Be("noop");
        job.ComponentVersion.Should().Be(3);
        job.Attempts.Should().Be(0);
        job.StartedUtc.Should().BeNull();
    }

    [Fact]
    public async Task Enqueueing_a_known_unit_updates_it_instead_of_adding_a_row()
    {
        await using var harness = await JobHarness.CreateAsync(postgres, new NoopHandler(1));

        await harness.EnqueueAsync("noop", "target-1");

        await using (var context = harness.CreateContext())
        {
            var failed = await context.Jobs.SingleAsync();
            failed.State = JobState.Failed;
            failed.Error = "it went wrong";
            failed.FinishedUtc = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync();
        }

        await harness.EnqueueAsync("noop", "target-1");

        await using var reader = harness.CreateContext();
        var job = await reader.Jobs.SingleAsync();

        job.State.Should().Be(JobState.Pending);
        job.Error.Should().BeNull("a re-queued unit must not display the last run's failure");
        job.FinishedUtc.Should().BeNull();
    }

    [Fact]
    public async Task The_payload_round_trips_as_json()
    {
        await using var harness = await JobHarness.CreateAsync(postgres, new NoopHandler(1));

        await harness.EnqueueAsync(
            "noop",
            "target-1",
            new { Path = "/tmp/a.zip", SizeBytes = 42L }
        );

        await using var reader = harness.CreateContext();
        var job = await reader.Jobs.SingleAsync();

        job.Payload.Should().Contain("\"path\"").And.Contain("/tmp/a.zip");
    }

    [Fact]
    public async Task Re_queueing_without_a_payload_keeps_the_one_already_stored()
    {
        await using var harness = await JobHarness.CreateAsync(postgres, new NoopHandler(1));

        await harness.EnqueueAsync("noop", "target-1", new { Path = "/tmp/a.zip" });
        await harness.EnqueueAsync("noop", "target-1");

        await using var reader = harness.CreateContext();

        // The jobs page's retry button has no payload to give. Nulling it here would strand an
        // unpack job's only record of where its archive is.
        (await reader.Jobs.SingleAsync()).Payload.Should().Contain("/tmp/a.zip");
    }

    [Fact]
    public async Task Enqueueing_an_unregistered_kind_is_refused()
    {
        await using var harness = await JobHarness.CreateAsync(postgres, new NoopHandler(1));

        var act = async () => await harness.EnqueueAsync("classify", "target-1");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Enqueueing_wakes_the_pump()
    {
        await using var harness = await JobHarness.CreateAsync(postgres, new NoopHandler(1));

        await harness.EnqueueAsync("noop", "target-1");

        // Already signalled, so this returns without waiting out the timeout.
        await harness
            .Resolve<JobSignal>()
            .WaitAsync(TimeSpan.FromSeconds(5), CancellationToken.None);
    }

    private sealed class NoopHandler(int version) : IJobHandler
    {
        public string Kind => "noop";

        public string ComponentId => "noop";

        public int Version => version;

        public Task ExecuteAsync(JobContext context, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
```

Add `using Fip.Strive.Application.Features.Jobs.Services;` for `JobSignal`.

- [ ] **Step 3: Run the test to verify it fails**

```bash
dotnet test src/Fip.Strive.IntegrationTests --filter JobQueueTests
```

Expected: compile failure — `JobQueue`, `JobSignal`, `IJobQueue` do not exist.

- [ ] **Step 4: Write the signal**

`src/Fip.Strive.Application/Features/Jobs/Services/JobSignal.cs`:

```csharp
namespace Fip.Strive.Application.Features.Jobs.Services;

/// <summary>
/// Wakes the pump when work is enqueued. A single latching slot rather than a counter: the pump
/// drains whatever it finds, so ten enqueues between two wake-ups need one wake-up, not ten.
/// </summary>
public sealed class JobSignal : IDisposable
{
    private readonly SemaphoreSlim _slot = new(0, 1);

    public void Set()
    {
        try
        {
            if (_slot.CurrentCount == 0)
                _slot.Release();
        }
        catch (SemaphoreFullException)
        {
            // Two callers passed the check together. The slot is set, which is all either wanted;
            // the maximum of one is what turns the race into this exception rather than a counter
            // that climbs and makes the pump spin.
        }
    }

    /// <summary>
    /// Returns when signalled or when <paramref name="timeout"/> elapses. The timeout is the poll
    /// interval, so a lost signal costs latency rather than a stuck queue.
    /// </summary>
    public async Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken) =>
        await _slot.WaitAsync(timeout, cancellationToken);

    public void Dispose() => _slot.Dispose();
}
```

- [ ] **Step 5: Write the queue contract**

`src/Fip.Strive.Application/Features/Jobs/Services/Contracts/IJobQueue.cs`:

```csharp
namespace Fip.Strive.Application.Features.Jobs.Services.Contracts;

public interface IJobQueue
{
    /// <summary>
    /// Upserts the work unit to <c>Pending</c> and wakes the pump. Enqueueing a unit that already
    /// exists resets it rather than adding a row.
    /// </summary>
    /// <param name="payload">
    /// Serialized to JSON. Omit it to keep whatever is already stored — which is what a retry from
    /// the UI does, having no payload of its own to supply.
    /// </param>
    /// <returns>The unit's id, whether it was created now or already existed.</returns>
    Task<Guid> EnqueueAsync(
        string kind,
        string targetKey,
        object? payload = null,
        CancellationToken cancellationToken = default
    );
}
```

- [ ] **Step 6: Write the queue**

`src/Fip.Strive.Application/Features/Jobs/Services/JobQueue.cs`:

```csharp
using System.Text.Json;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Fip.Strive.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Application.Features.Jobs.Services;

public sealed class JobQueue(
    StriveContext context,
    IJobRegistry registry,
    JobSignal signal,
    TimeProvider timeProvider,
    ILogger<JobQueue> logger
) : IJobQueue
{
    private static readonly JsonSerializerOptions PayloadOptions = new(JsonSerializerDefaults.Web);

    public async Task<Guid> EnqueueAsync(
        string kind,
        string targetKey,
        object? payload = null,
        CancellationToken cancellationToken = default
    )
    {
        // Resolved before anything is written, so an unregistered kind fails at the call site
        // rather than becoming a row nothing can ever claim.
        var component = registry.Resolve(kind);
        var now = timeProvider.GetUtcNow();
        var json = payload is null ? null : JsonSerializer.Serialize(payload, PayloadOptions);

        var existing = await context.Jobs.FirstOrDefaultAsync(
            job => job.Kind == kind && job.TargetKey == targetKey,
            cancellationToken
        );

        if (existing is not null)
        {
            existing.ComponentId = component.ComponentId;
            existing.ComponentVersion = component.Version;
            existing.State = JobState.Pending;

            // Only when the caller brought one. A retry has no payload to give, and overwriting
            // with null would lose the handler's only input.
            if (json is not null)
                existing.Payload = json;

            // The previous run's outcome is not this run's. Leaving it would have the jobs page
            // showing an error against a job that is queued.
            existing.Error = null;
            existing.ProgressCurrent = null;
            existing.ProgressTotal = null;
            existing.ProgressNote = null;
            existing.EnqueuedUtc = now;
            existing.StartedUtc = null;
            existing.FinishedUtc = null;

            await context.SaveChangesAsync(cancellationToken);
            signal.Set();

            logger.LogInformation("Re-queued {Kind} job for {TargetKey}", kind, targetKey);
            return existing.Id;
        }

        var job = new Job
        {
            Id = Guid.CreateVersion7(),
            Kind = kind,
            TargetKey = targetKey,
            ComponentId = component.ComponentId,
            ComponentVersion = component.Version,
            State = JobState.Pending,
            Payload = json,
            EnqueuedUtc = now,
        };

        context.Jobs.Add(job);
        await context.SaveChangesAsync(cancellationToken);

        // Signalled only after the commit, so the pump can never wake for a row it cannot see.
        signal.Set();

        logger.LogInformation("Queued {Kind} job {JobId} for {TargetKey}", kind, job.Id, targetKey);
        return job.Id;
    }
}
```

- [ ] **Step 7: Run the tests to verify they pass**

```bash
dotnet test src/Fip.Strive.IntegrationTests --filter JobQueueTests
```

Expected: 6 passed.

- [ ] **Step 8: Format and commit**

```bash
dotnet csharpier format src/
git add src/
git commit -m "feat: enqueue jobs as an upsert on the work unit"
```

---

### Task 4: Claiming, completion and startup recovery

**Files:**
- Create: `src/Fip.Strive.Application/Features/Jobs/Services/Contracts/IJobStore.cs`
- Create: `src/Fip.Strive.Application/Features/Jobs/Services/JobStore.cs`
- Modify: `src/Fip.Strive.IntegrationTests/Fixtures/JobHarness.cs`
- Test: `src/Fip.Strive.IntegrationTests/JobStoreTests.cs`

**Interfaces:**
- Consumes: `Job`, `JobState` (Task 1); `JobProgress` (Task 2).
- Produces: `IJobStore` with `Task<IReadOnlyList<Guid>> ClaimAsync(int max, CancellationToken)`, `Task<Job?> GetAsync(Guid, CancellationToken)`, `Task CompleteAsync(Guid, CancellationToken)`, `Task FailAsync(Guid, string error, CancellationToken)`, `Task ReleaseAsync(Guid, CancellationToken)`, `Task SaveProgressAsync(Guid, JobProgress, CancellationToken)`, `Task<int> RecoverInterruptedAsync(CancellationToken)`; `JobHarness.SeedAsync(...)`.

- [ ] **Step 1: Extend the harness**

Add to `src/Fip.Strive.IntegrationTests/Fixtures/JobHarness.cs`, and add `using Fip.Strive.Application.Features.Jobs.Models;`:

```csharp
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
```

Register the store in the harness's provider, next to `IJobQueue`:

```csharp
        services.AddScoped<IJobStore, JobStore>();
```

- [ ] **Step 2: Write the failing test**

Create `src/Fip.Strive.IntegrationTests/JobStoreTests.cs`:

```csharp
using AwesomeAssertions;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Fip.Strive.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.IntegrationTests;

[Collection(PostgresCollection.Name)]
public class JobStoreTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Claiming_marks_the_job_running_and_counts_the_attempt()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);
        var id = await harness.SeedAsync("noop", "target-1", JobState.Pending);

        await using var scope = harness.Scope();
        var claimed = await Store(scope).ClaimAsync(10, CancellationToken.None);

        claimed.Should().BeEquivalentTo([id]);

        await using var reader = harness.CreateContext();
        var job = await reader.Jobs.SingleAsync();

        job.State.Should().Be(JobState.Running);
        job.Attempts.Should().Be(1);
        job.StartedUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Claiming_takes_no_more_than_it_asked_for_and_takes_the_oldest_first()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);

        var first = await harness.SeedAsync(
            "noop",
            "a",
            JobState.Pending,
            enqueued: DateTimeOffset.UtcNow.AddMinutes(-5)
        );
        await harness.SeedAsync(
            "noop",
            "b",
            JobState.Pending,
            enqueued: DateTimeOffset.UtcNow.AddMinutes(-1)
        );

        await using var scope = harness.Scope();

        (await Store(scope).ClaimAsync(1, CancellationToken.None)).Should().BeEquivalentTo([first]);
    }

    [Fact]
    public async Task Two_claims_never_hand_the_same_job_to_two_workers()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);

        for (var index = 0; index < 20; index++)
            await harness.SeedAsync("noop", $"target-{index}", JobState.Pending);

        await using var one = harness.Scope();
        await using var two = harness.Scope();

        // Run together on separate connections: SKIP LOCKED is what has to keep them disjoint.
        var results = await Task.WhenAll(
            Store(one).ClaimAsync(20, CancellationToken.None),
            Store(two).ClaimAsync(20, CancellationToken.None)
        );

        var all = results.SelectMany(ids => ids).ToList();

        all.Should().HaveCount(20);
        all.Distinct().Should().HaveCount(20, "no job may be claimed twice");
    }

    [Fact]
    public async Task Only_pending_jobs_are_claimed()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);

        await harness.SeedAsync("noop", "running", JobState.Running);
        await harness.SeedAsync("noop", "succeeded", JobState.Succeeded);
        await harness.SeedAsync("noop", "failed", JobState.Failed);

        await using var scope = harness.Scope();

        (await Store(scope).ClaimAsync(10, CancellationToken.None)).Should().BeEmpty();
    }

    [Fact]
    public async Task Recovery_requeues_interrupted_and_stale_jobs_without_spending_an_attempt()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);

        var interrupted = await harness.SeedAsync(
            "noop",
            "interrupted",
            JobState.Running,
            attempts: 1
        );
        await harness.SeedAsync("noop", "stale", JobState.Stale);
        await harness.SeedAsync("noop", "failed", JobState.Failed);

        await using var scope = harness.Scope();

        (await Store(scope).RecoverInterruptedAsync(CancellationToken.None)).Should().Be(2);

        await using var reader = harness.CreateContext();

        var job = await reader.Jobs.SingleAsync(row => row.Id == interrupted);
        job.State.Should().Be(JobState.Pending);
        job.StartedUtc.Should().BeNull();
        job.Attempts.Should().Be(1, "a kill is not a failed attempt");

        var failed = await reader.Jobs.SingleAsync(row => row.TargetKey == "failed");
        failed.State.Should().Be(JobState.Failed, "a parked failure waits for a manual retry");
    }

    [Fact]
    public async Task Completing_and_failing_write_the_terminal_state()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);
        var succeeded = await harness.SeedAsync("noop", "ok", JobState.Running);
        var failed = await harness.SeedAsync("noop", "bad", JobState.Running);

        await using (var scope = harness.Scope())
        {
            await Store(scope).CompleteAsync(succeeded, CancellationToken.None);
            await Store(scope).FailAsync(failed, "disk on fire", CancellationToken.None);
        }

        await using var reader = harness.CreateContext();

        var ok = await reader.Jobs.SingleAsync(row => row.Id == succeeded);
        ok.State.Should().Be(JobState.Succeeded);
        ok.FinishedUtc.Should().NotBeNull();
        ok.Error.Should().BeNull();

        var bad = await reader.Jobs.SingleAsync(row => row.Id == failed);
        bad.State.Should().Be(JobState.Failed);
        bad.Error.Should().Be("disk on fire");
        bad.FinishedUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Releasing_returns_a_job_to_the_queue_untouched()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);
        var id = await harness.SeedAsync("noop", "target-1", JobState.Running, attempts: 1);

        await using (var scope = harness.Scope())
            await Store(scope).ReleaseAsync(id, CancellationToken.None);

        await using var reader = harness.CreateContext();
        var job = await reader.Jobs.SingleAsync();

        job.State.Should().Be(JobState.Pending);
        job.Attempts.Should().Be(1, "shutdown is not a failed attempt");
        job.Error.Should().BeNull();
    }

    [Fact]
    public async Task Progress_is_stored_against_the_job()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);
        var id = await harness.SeedAsync("noop", "target-1", JobState.Running);

        await using (var scope = harness.Scope())
            await Store(scope)
                .SaveProgressAsync(id, new JobProgress(7, 40, "a/b.json"), CancellationToken.None);

        await using var reader = harness.CreateContext();
        var job = await reader.Jobs.SingleAsync();

        job.ProgressCurrent.Should().Be(7);
        job.ProgressTotal.Should().Be(40);
        job.ProgressNote.Should().Be("a/b.json");
    }

    private static IJobStore Store(AsyncServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IJobStore>();
}
```

- [ ] **Step 3: Run the test to verify it fails**

```bash
dotnet test src/Fip.Strive.IntegrationTests --filter JobStoreTests
```

Expected: compile failure — `IJobStore` and `JobStore` do not exist.

- [ ] **Step 4: Write the store contract**

`src/Fip.Strive.Application/Features/Jobs/Services/Contracts/IJobStore.cs`:

```csharp
using Fip.Strive.Application.Features.Jobs.Models;

namespace Fip.Strive.Application.Features.Jobs.Services.Contracts;

/// <summary>
/// The runner's persistence surface. Separate from <see cref="IJobQueue"/> because enqueueing is
/// something the app does and claiming is something only the runner does.
/// </summary>
public interface IJobStore
{
    /// <summary>
    /// Atomically claims up to <paramref name="max"/> pending jobs, marking them running. Returns
    /// the ids actually claimed.
    /// </summary>
    Task<IReadOnlyList<Guid>> ClaimAsync(int max, CancellationToken cancellationToken);

    Task<Job?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task CompleteAsync(Guid id, CancellationToken cancellationToken);

    Task FailAsync(Guid id, string error, CancellationToken cancellationToken);

    /// <summary>
    /// Returns a claimed job to <c>Pending</c> without recording a failure. Used when shutdown
    /// interrupts a handler — the work was not attempted and lost, it was stopped.
    /// </summary>
    Task ReleaseAsync(Guid id, CancellationToken cancellationToken);

    Task SaveProgressAsync(Guid id, JobProgress progress, CancellationToken cancellationToken);

    /// <summary>
    /// Re-queues everything left <c>Running</c> by a previous process, plus anything marked
    /// <c>Stale</c>. Returns how many rows were affected.
    /// </summary>
    Task<int> RecoverInterruptedAsync(CancellationToken cancellationToken);
}
```

- [ ] **Step 5: Write the store**

`src/Fip.Strive.Application/Features/Jobs/Services/JobStore.cs`:

```csharp
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Fip.Strive.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Application.Features.Jobs.Services;

public sealed class JobStore(
    StriveContext context,
    TimeProvider timeProvider,
    ILogger<JobStore> logger
) : IJobStore
{
    /// <summary>
    /// One statement, so the select and the update cannot be separated by another claimer.
    /// <c>SKIP LOCKED</c> is what makes concurrent pumps take disjoint sets instead of blocking on
    /// each other — and the reason this is raw SQL rather than EF.
    /// </summary>
    private const string ClaimSql = """
        UPDATE jobs
        SET state = 'Running', started_utc = {0}, attempts = attempts + 1
        WHERE id IN (
            SELECT id FROM jobs
            WHERE state = 'Pending'
            ORDER BY enqueued_utc
            LIMIT {1}
            FOR UPDATE SKIP LOCKED
        )
        RETURNING id AS "Value"
        """;

    public async Task<IReadOnlyList<Guid>> ClaimAsync(int max, CancellationToken cancellationToken)
    {
        if (max <= 0)
            return [];

        return await context
            .Database.SqlQueryRaw<Guid>(ClaimSql, timeProvider.GetUtcNow(), max)
            .ToListAsync(cancellationToken);
    }

    public Task<Job?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        context.Jobs.AsNoTracking().FirstOrDefaultAsync(job => job.Id == id, cancellationToken);

    public Task CompleteAsync(Guid id, CancellationToken cancellationToken) =>
        context
            .Jobs.Where(job => job.Id == id)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(job => job.State, JobState.Succeeded)
                        .SetProperty(job => job.FinishedUtc, timeProvider.GetUtcNow())
                        .SetProperty(job => job.Error, (string?)null),
                cancellationToken
            );

    public async Task FailAsync(Guid id, string error, CancellationToken cancellationToken)
    {
        await context
            .Jobs.Where(job => job.Id == id)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(job => job.State, JobState.Failed)
                        .SetProperty(job => job.FinishedUtc, timeProvider.GetUtcNow())
                        .SetProperty(job => job.Error, Truncate(error)),
                cancellationToken
            );

        logger.LogError("Job {JobId} failed: {Error}", id, error);
    }

    public Task ReleaseAsync(Guid id, CancellationToken cancellationToken) =>
        context
            .Jobs.Where(job => job.Id == id)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(job => job.State, JobState.Pending)
                        .SetProperty(job => job.StartedUtc, (DateTimeOffset?)null),
                cancellationToken
            );

    public Task SaveProgressAsync(
        Guid id,
        JobProgress progress,
        CancellationToken cancellationToken
    ) =>
        context
            .Jobs.Where(job => job.Id == id)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(job => job.ProgressCurrent, progress.Current)
                        .SetProperty(job => job.ProgressTotal, progress.Total)
                        .SetProperty(job => job.ProgressNote, progress.Note),
                cancellationToken
            );

    public async Task<int> RecoverInterruptedAsync(CancellationToken cancellationToken)
    {
        // Attempts is deliberately untouched. Only one process runs, so a Running row here was
        // killed rather than tried — charging it an attempt would turn a restart into a permanent
        // failure under the one-attempt retry policy.
        var recovered = await context
            .Jobs.Where(job => job.State == JobState.Running || job.State == JobState.Stale)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(job => job.State, JobState.Pending)
                        .SetProperty(job => job.StartedUtc, (DateTimeOffset?)null),
                cancellationToken
            );

        if (recovered > 0)
            logger.LogInformation("Re-queued {Count} interrupted or stale jobs", recovered);

        return recovered;
    }

    /// <summary>
    /// Exception messages are unbounded and this one is only ever read by a human on the jobs
    /// page. The full detail is in the log next to it.
    /// </summary>
    private static string Truncate(string error) => error.Length <= 2000 ? error : error[..2000];
}
```

- [ ] **Step 6: Run the tests to verify they pass**

```bash
dotnet test src/Fip.Strive.IntegrationTests --filter JobStoreTests
```

Expected: 8 passed.

- [ ] **Step 7: Format and commit**

```bash
dotnet csharpier format src/
git add src/
git commit -m "feat: claim jobs with SKIP LOCKED and recover interrupted ones"
```

---

### Task 5: Progress throttle and change notifications

**Files:**
- Create: `src/Fip.Strive.Application/Features/Jobs/Services/ThrottledProgress.cs`
- Create: `src/Fip.Strive.Application/Features/Jobs/Services/Contracts/IJobNotifier.cs`
- Create: `src/Fip.Strive.Application/Features/Jobs/Services/JobNotifier.cs`
- Create: `src/Fip.Strive.Application.UnitTests/Fixtures/StubClock.cs`
- Test: `src/Fip.Strive.Application.UnitTests/Jobs/ThrottledProgressTests.cs`
- Test: `src/Fip.Strive.Application.UnitTests/Jobs/JobNotifierTests.cs`

**Interfaces:**
- Consumes: `JobProgress` (Task 2).
- Produces: `ThrottledProgress(Func<JobProgress, Task> write, TimeSpan interval, TimeProvider clock)` implementing `IProgress<JobProgress>` with `Task FlushAsync()`; `IJobNotifier` with `void Notify()` and `IDisposable Subscribe(Action handler)`; `JobNotifier`; `StubClock`.

`Report` is fire-and-forget because `IProgress<T>.Report` is void and a handler must never block on a database write to say where it is. `FlushAsync` is awaited, because the last write has to land before the job is marked finished — otherwise a completed job sits displaying "1 of 40" forever.

- [ ] **Step 1: Write the failing tests**

Create `src/Fip.Strive.Application.UnitTests/Fixtures/StubClock.cs`:

```csharp
namespace Fip.Strive.Application.UnitTests.Fixtures;

/// <summary>A clock the test moves by hand, so throttling can be asserted without sleeping.</summary>
public sealed class StubClock(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now;

    public DateTimeOffset Advance(TimeSpan step) => Now += step;
}
```

Create `src/Fip.Strive.Application.UnitTests/Jobs/ThrottledProgressTests.cs`:

```csharp
using AwesomeAssertions;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services;
using Fip.Strive.Application.UnitTests.Fixtures;

namespace Fip.Strive.Application.UnitTests.Jobs;

public class ThrottledProgressTests
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(500);

    [Fact]
    public void The_first_report_is_written_immediately()
    {
        var progress = Create(out var written, out _);

        progress.Report(new JobProgress(1, 10));

        written.Should().HaveCount(1);
    }

    [Fact]
    public void Reports_inside_the_interval_are_held_back()
    {
        var progress = Create(out var written, out var clock);

        progress.Report(new JobProgress(1, 10));
        clock.Advance(TimeSpan.FromMilliseconds(100));
        progress.Report(new JobProgress(2, 10));
        clock.Advance(TimeSpan.FromMilliseconds(100));
        progress.Report(new JobProgress(3, 10));

        // Forty thousand files must not become forty thousand UPDATEs.
        written.Should().HaveCount(1);
    }

    [Fact]
    public void A_report_after_the_interval_is_written()
    {
        var progress = Create(out var written, out var clock);

        progress.Report(new JobProgress(1, 10));
        clock.Advance(Interval);
        progress.Report(new JobProgress(9, 10));

        written.Should().HaveCount(2);
        written[1].Current.Should().Be(9);
    }

    [Fact]
    public async Task Flushing_writes_the_last_held_report()
    {
        var progress = Create(out var written, out var clock);

        progress.Report(new JobProgress(1, 10));
        clock.Advance(TimeSpan.FromMilliseconds(10));
        progress.Report(new JobProgress(10, 10));

        await progress.FlushAsync();

        // Without this a finished job would sit displaying 1 of 10 forever.
        written.Should().HaveCount(2);
        written[1].Current.Should().Be(10);
    }

    [Fact]
    public async Task Flushing_with_nothing_held_writes_nothing()
    {
        var progress = Create(out var written, out _);

        progress.Report(new JobProgress(1, 10));
        await progress.FlushAsync();
        await progress.FlushAsync();

        written.Should().HaveCount(1);
    }

    [Fact]
    public void A_failing_write_does_not_reach_the_handler()
    {
        var progress = new ThrottledProgress(
            _ => throw new InvalidOperationException("the database is gone"),
            Interval,
            new StubClock(DateTimeOffset.UnixEpoch)
        );

        // Progress is advisory. A handler must not die because its position could not be recorded.
        var act = () => progress.Report(new JobProgress(1, 10));

        act.Should().NotThrow();
    }

    private static ThrottledProgress Create(out List<JobProgress> written, out StubClock clock)
    {
        var captured = new List<JobProgress>();
        written = captured;
        clock = new StubClock(DateTimeOffset.UnixEpoch);

        // Completes synchronously, so assertions do not have to wait on the fire-and-forget path.
        return new ThrottledProgress(
            value =>
            {
                captured.Add(value);
                return Task.CompletedTask;
            },
            Interval,
            clock
        );
    }
}
```

Create `src/Fip.Strive.Application.UnitTests/Jobs/JobNotifierTests.cs`:

```csharp
using AwesomeAssertions;
using Fip.Strive.Application.Features.Jobs.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Fip.Strive.Application.UnitTests.Jobs;

public class JobNotifierTests
{
    [Fact]
    public void Subscribers_are_told_something_changed()
    {
        var notifier = new JobNotifier(NullLogger<JobNotifier>.Instance);
        var calls = 0;

        using var subscription = notifier.Subscribe(() => calls++);
        notifier.Notify();

        calls.Should().Be(1);
    }

    [Fact]
    public void Disposing_a_subscription_stops_the_callbacks()
    {
        var notifier = new JobNotifier(NullLogger<JobNotifier>.Instance);
        var calls = 0;

        var subscription = notifier.Subscribe(() => calls++);
        subscription.Dispose();
        notifier.Notify();

        calls.Should().Be(0);
    }

    [Fact]
    public void One_faulted_subscriber_does_not_stop_the_others()
    {
        var notifier = new JobNotifier(NullLogger<JobNotifier>.Instance);
        var reached = false;

        using var bad = notifier.Subscribe(() =>
            throw new InvalidOperationException("circuit is gone")
        );
        using var good = notifier.Subscribe(() => reached = true);

        notifier.Notify();

        // A dead browser circuit must never be able to stall the runner.
        reached.Should().BeTrue();
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

```bash
dotnet test src/Fip.Strive.Application.UnitTests --filter "ThrottledProgressTests|JobNotifierTests"
```

Expected: compile failure — `ThrottledProgress` and `JobNotifier` do not exist.

- [ ] **Step 3: Write the throttle**

`src/Fip.Strive.Application/Features/Jobs/Services/ThrottledProgress.cs`:

```csharp
using Fip.Strive.Application.Features.Jobs.Models;

namespace Fip.Strive.Application.Features.Jobs.Services;

/// <summary>
/// Rate-limits a handler's progress reports on the way to the database. A handler reports as often
/// as is natural for it — once per file, for unpacking — and this decides how much of that is
/// worth a write.
/// </summary>
public sealed class ThrottledProgress(
    Func<JobProgress, Task> write,
    TimeSpan interval,
    TimeProvider clock
) : IProgress<JobProgress>
{
    private readonly Lock _gate = new();

    private DateTimeOffset _lastWrite = DateTimeOffset.MinValue;
    private JobProgress? _held;

    public void Report(JobProgress value)
    {
        lock (_gate)
        {
            var now = clock.GetUtcNow();

            if (now - _lastWrite < interval)
            {
                // Held rather than dropped, so the flush can write the final position.
                _held = value;
                return;
            }

            _lastWrite = now;
            _held = null;
        }

        // Not awaited: IProgress.Report is void by contract, and a handler blocking on a database
        // write to say where it is would make reporting cost more than the work being reported.
        _ = WriteSafelyAsync(value);
    }

    /// <summary>
    /// Writes whatever the throttle is holding, and waits for it. Called before a job's terminal
    /// state is written, so the last position the UI shows is the real one.
    /// </summary>
    public async Task FlushAsync()
    {
        JobProgress? held;

        lock (_gate)
        {
            held = _held;
            _held = null;

            if (held is not null)
                _lastWrite = clock.GetUtcNow();
        }

        if (held is not null)
            await WriteSafelyAsync(held.Value);
    }

    /// <summary>
    /// Progress is advisory. Losing a position is not worth failing the job that reported it, and
    /// a throw on the fire-and-forget path would be unobserved anyway.
    /// </summary>
    private async Task WriteSafelyAsync(JobProgress value)
    {
        try
        {
            await write(value);
        }
        catch (Exception)
        {
            // The caller logs; there is nothing useful to do here.
        }
    }
}
```

- [ ] **Step 4: Write the notifier**

`src/Fip.Strive.Application/Features/Jobs/Services/Contracts/IJobNotifier.cs`:

```csharp
namespace Fip.Strive.Application.Features.Jobs.Services.Contracts;

/// <summary>
/// Tells subscribers that the job table changed, and nothing more. Carrying the changed row would
/// give the UI a second representation of it that can disagree with the table after a dropped or
/// reordered notification; re-reading cannot.
/// </summary>
public interface IJobNotifier
{
    void Notify();

    IDisposable Subscribe(Action handler);
}
```

`src/Fip.Strive.Application/Features/Jobs/Services/JobNotifier.cs`:

```csharp
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Application.Features.Jobs.Services;

public sealed class JobNotifier(ILogger<JobNotifier> logger) : IJobNotifier
{
    private readonly Lock _gate = new();
    private readonly List<Action> _handlers = [];

    public void Notify()
    {
        Action[] handlers;

        lock (_gate)
        {
            handlers = [.. _handlers];
        }

        foreach (var handler in handlers)
        {
            try
            {
                handler();
            }
            catch (Exception exception)
            {
                // A circuit that went away mid-notification is ordinary. The runner must not care.
                logger.LogDebug(exception, "A job subscriber faulted and was skipped");
            }
        }
    }

    public IDisposable Subscribe(Action handler)
    {
        lock (_gate)
        {
            _handlers.Add(handler);
        }

        return new Subscription(this, handler);
    }

    private void Unsubscribe(Action handler)
    {
        lock (_gate)
        {
            _handlers.Remove(handler);
        }
    }

    private sealed class Subscription(JobNotifier notifier, Action handler) : IDisposable
    {
        public void Dispose() => notifier.Unsubscribe(handler);
    }
}
```

- [ ] **Step 5: Run the tests to verify they pass**

```bash
dotnet test src/Fip.Strive.Application.UnitTests --filter "ThrottledProgressTests|JobNotifierTests"
```

Expected: 9 passed.

- [ ] **Step 6: Format and commit**

```bash
dotnet csharpier format src/
git add src/
git commit -m "feat: throttle job progress and notify subscribers of changes"
```

---

### Task 6: The runner

**Files:**
- Create: `src/Fip.Strive.Application/Features/Jobs/JobOptions.cs`
- Create: `src/Fip.Strive.Application/Features/Jobs/Services/JobRunner.cs`
- Modify: `src/Directory.Packages.props`
- Modify: `src/Fip.Strive.Application/Fip.Strive.Application.csproj`
- Modify: `src/Fip.Strive.Application/Registration.cs`
- Modify: `src/Fip.Strive.IntegrationTests/Fixtures/JobHarness.cs`
- Modify: `src/Fip.Strive.IntegrationTests/Fixtures/StriveAppFactory.cs`
- Test: `src/Fip.Strive.IntegrationTests/JobRunnerTests.cs`

**Interfaces:**
- Consumes: `IJobStore` (Task 4); `IJobHandler`, `JobContext` (Task 2); `JobSignal` (Task 3); `ThrottledProgress`, `IJobNotifier` (Task 5).
- Produces: `JobOptions` with `SectionName`, `Enabled`, `Parallelism`, `PollInterval`, `ProgressInterval`; `JobRunner : BackgroundService`; `Registration` wiring; `JobHarness.CreateRunner()` and `JobHarness.RunUntilIdleAsync()`.

- [ ] **Step 1: Add the hosting package**

In `src/Directory.Packages.props`, inside the `Application` `ItemGroup`, after the Logging abstractions entry:

```xml
    <!-- BackgroundService only. Abstractions, so the application layer stays host-agnostic. -->
    <PackageVersion Include="Microsoft.Extensions.Hosting.Abstractions" Version="10.0.11" />
```

In `src/Fip.Strive.Application/Fip.Strive.Application.csproj`, add to the `ItemGroup`:

```xml
    <PackageReference Include="Microsoft.Extensions.Hosting.Abstractions" />
```

- [ ] **Step 2: Write the options**

`src/Fip.Strive.Application/Features/Jobs/JobOptions.cs`:

```csharp
namespace Fip.Strive.Application.Features.Jobs;

public sealed class JobOptions
{
    public const string SectionName = "Jobs";

    /// <summary>
    /// Whether the runner starts. Off in tests that boot the host but drive jobs themselves, so a
    /// background runner cannot race the thing under test.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Concurrent workers. Defaults to the host's processor count capped at eight — the target box
    /// has eight threads and nothing here benefits from oversubscribing them.
    /// </summary>
    public int Parallelism { get; set; } = Math.Min(Environment.ProcessorCount, 8);

    /// <summary>
    /// How long the pump waits for a signal before looking anyway. The safety net that turns a
    /// lost signal into latency rather than a stuck queue.
    /// </summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Floor between persisted progress writes. Without it a forty-thousand-file unpack would cost
    /// forty thousand UPDATEs.
    /// </summary>
    public TimeSpan ProgressInterval { get; set; } = TimeSpan.FromMilliseconds(500);
}
```

- [ ] **Step 3: Extend the harness**

Add to `src/Fip.Strive.IntegrationTests/Fixtures/JobHarness.cs`. In the constructor, next to the other registrations:

```csharp
        services.AddSingleton<IJobNotifier, JobNotifier>();

        // Short poll so a test never waits five seconds for the pump to look again.
        services.Configure<JobOptions>(options =>
            options.PollInterval = TimeSpan.FromMilliseconds(100)
        );
```

and as members:

```csharp
    public JobRunner CreateRunner() =>
        new(
            _provider,
            Resolve<JobSignal>(),
            Resolve<IJobNotifier>(),
            Resolve<IOptions<JobOptions>>(),
            TimeProvider.System,
            Resolve<ILogger<JobRunner>>()
        );

    /// <summary>
    /// Starts a runner, waits for the queue to drain, then stops it. Bounded by a hard timeout so
    /// a hung handler fails the test rather than the suite.
    /// </summary>
    public async Task RunUntilIdleAsync(TimeSpan? timeout = null)
    {
        using var runner = CreateRunner();
        await runner.StartAsync(CancellationToken.None);

        var deadline = DateTimeOffset.UtcNow + (timeout ?? TimeSpan.FromSeconds(60));

        while (DateTimeOffset.UtcNow < deadline)
        {
            await using var context = CreateContext();

            var outstanding = await context.Jobs.CountAsync(job =>
                job.State == JobState.Pending || job.State == JobState.Running
            );

            if (outstanding == 0)
                break;

            await Task.Delay(50);
        }

        await runner.StopAsync(CancellationToken.None);
    }
```

Add these usings to the file:

```csharp
using Fip.Strive.Application.Features.Jobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
```

`using var`, not `await using`: `BackgroundService` implements `IDisposable` only.

- [ ] **Step 4: Write the failing test**

Create `src/Fip.Strive.IntegrationTests/JobRunnerTests.cs`:

```csharp
using System.Collections.Concurrent;
using AwesomeAssertions;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Fip.Strive.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.IntegrationTests;

[Collection(PostgresCollection.Name)]
public class JobRunnerTests(PostgresFixture postgres)
{
    [Fact]
    public async Task An_enqueued_job_runs_and_succeeds()
    {
        var handler = new RecordingHandler();
        await using var harness = await JobHarness.CreateAsync(postgres, handler);

        await harness.EnqueueAsync("noop", "target-1");
        await harness.RunUntilIdleAsync();

        handler.Targets.Should().BeEquivalentTo(["target-1"]);

        await using var reader = harness.CreateContext();
        var job = await reader.Jobs.SingleAsync();

        job.State.Should().Be(JobState.Succeeded);
        job.FinishedUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task A_throwing_handler_parks_the_job_with_its_error()
    {
        await using var harness = await JobHarness.CreateAsync(
            postgres,
            new ThrowingHandler("the reader exploded")
        );

        await harness.EnqueueAsync("noop", "target-1");
        await harness.RunUntilIdleAsync();

        await using var reader = harness.CreateContext();
        var job = await reader.Jobs.SingleAsync();

        job.State.Should().Be(JobState.Failed);
        job.Error.Should().Contain("the reader exploded");
        job.Attempts.Should().Be(1, "one attempt, then it waits for a human");
    }

    [Fact]
    public async Task A_job_left_running_by_a_previous_process_is_picked_up_on_start()
    {
        var handler = new RecordingHandler();
        await using var harness = await JobHarness.CreateAsync(postgres, handler);

        await harness.SeedAsync("noop", "interrupted", JobState.Running, attempts: 1);
        await harness.RunUntilIdleAsync();

        handler.Targets.Should().BeEquivalentTo(["interrupted"]);

        await using var reader = harness.CreateContext();
        (await reader.Jobs.SingleAsync()).State.Should().Be(JobState.Succeeded);
    }

    [Fact]
    public async Task A_disabled_runner_executes_nothing()
    {
        var handler = new RecordingHandler();
        await using var harness = await JobHarness.CreateAsync(postgres, handler);

        await harness.EnqueueAsync("noop", "target-1");
        await harness.RunUntilIdleAsync(TimeSpan.FromSeconds(2), enabled: false);

        handler.Targets.Should().BeEmpty();

        await using var reader = harness.CreateContext();
        (await reader.Jobs.SingleAsync()).State.Should().Be(JobState.Pending);
    }

    [Fact]
    public async Task Progress_reported_by_a_handler_reaches_the_table()
    {
        await using var harness = await JobHarness.CreateAsync(postgres, new ProgressHandler());

        await harness.EnqueueAsync("noop", "target-1");
        await harness.RunUntilIdleAsync();

        await using var reader = harness.CreateContext();
        var job = await reader.Jobs.SingleAsync();

        // The throttle holds the middle reports back; the flush before the terminal write is what
        // makes the last one land, so a finished job does not sit displaying 1 of 40.
        job.ProgressCurrent.Should().Be(40);
        job.ProgressTotal.Should().Be(40);
    }

    [Fact]
    public async Task Many_jobs_all_run_exactly_once()
    {
        var handler = new RecordingHandler();
        await using var harness = await JobHarness.CreateAsync(postgres, handler);

        for (var index = 0; index < 50; index++)
            await harness.EnqueueAsync("noop", $"target-{index}");

        await harness.RunUntilIdleAsync();

        handler.Targets.Should().HaveCount(50);
        handler.Targets.Distinct().Should().HaveCount(50, "no job may run twice");

        await using var reader = harness.CreateContext();
        (await reader.Jobs.CountAsync(job => job.State == JobState.Succeeded)).Should().Be(50);
    }

    private sealed class RecordingHandler : IJobHandler
    {
        private readonly ConcurrentBag<string> _targets = [];

        public string Kind => "noop";

        public string ComponentId => "noop";

        public int Version => 1;

        public IReadOnlyCollection<string> Targets => [.. _targets];

        public Task ExecuteAsync(JobContext context, CancellationToken cancellationToken)
        {
            _targets.Add(context.Job.TargetKey);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingHandler(string message) : IJobHandler
    {
        public string Kind => "noop";

        public string ComponentId => "noop";

        public int Version => 1;

        public Task ExecuteAsync(JobContext context, CancellationToken cancellationToken) =>
            throw new InvalidOperationException(message);
    }

    private sealed class ProgressHandler : IJobHandler
    {
        public string Kind => "noop";

        public string ComponentId => "noop";

        public int Version => 1;

        public Task ExecuteAsync(JobContext context, CancellationToken cancellationToken)
        {
            for (var index = 1; index <= 40; index++)
                context.Progress.Report(new JobProgress(index, 40, $"file-{index}.json"));

            return Task.CompletedTask;
        }
    }
}
```

`RunUntilIdleAsync` now needs an `enabled` parameter. Change its signature and body to:

```csharp
    public async Task RunUntilIdleAsync(TimeSpan? timeout = null, bool enabled = true)
    {
        Resolve<IOptions<JobOptions>>().Value.Enabled = enabled;

        using var runner = CreateRunner();
        await runner.StartAsync(CancellationToken.None);

        var deadline = DateTimeOffset.UtcNow + (timeout ?? TimeSpan.FromSeconds(60));

        while (DateTimeOffset.UtcNow < deadline)
        {
            await using var context = CreateContext();

            var outstanding = await context.Jobs.CountAsync(job =>
                job.State == JobState.Pending || job.State == JobState.Running
            );

            if (outstanding == 0)
                break;

            await Task.Delay(50);
        }

        await runner.StopAsync(CancellationToken.None);
    }
```

- [ ] **Step 5: Run the test to verify it fails**

```bash
dotnet test src/Fip.Strive.IntegrationTests --filter JobRunnerTests
```

Expected: compile failure — `JobRunner` does not exist.

- [ ] **Step 6: Write the runner**

`src/Fip.Strive.Application/Features/Jobs/Services/JobRunner.cs`:

```csharp
using System.Threading.Channels;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Application.Features.Jobs.Services;

/// <summary>
/// One pump and N workers. The pump owns the database claim; the workers own execution. Everything
/// runs in this process and nothing is tied to a browser circuit — closing the tab that started a
/// run has no effect on it.
/// </summary>
public sealed class JobRunner(
    IServiceProvider services,
    JobSignal signal,
    IJobNotifier notifier,
    IOptions<JobOptions> options,
    TimeProvider timeProvider,
    ILogger<JobRunner> logger
) : BackgroundService
{
    private readonly JobOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("The job runner is disabled; no jobs will be executed");
            return;
        }

        await RecoverAsync(stoppingToken);

        // Twice the worker count: enough that a worker finishing never waits on the pump, small
        // enough that rows do not sit marked Running in a buffer nobody is working on.
        var capacity = _options.Parallelism * 2;

        var channel = Channel.CreateBounded<Guid>(
            new BoundedChannelOptions(capacity)
            {
                SingleWriter = true,
                FullMode = BoundedChannelFullMode.Wait,
            }
        );

        var workers = Enumerable
            .Range(0, _options.Parallelism)
            .Select(_ => WorkAsync(channel.Reader, stoppingToken))
            .ToList();

        logger.LogInformation(
            "Job runner started with {Parallelism} workers",
            _options.Parallelism
        );

        await PumpAsync(channel.Writer, capacity, stoppingToken);
        channel.Writer.Complete();

        await Task.WhenAll(workers);
    }

    private async Task RecoverAsync(CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IJobStore>();

        if (await store.RecoverInterruptedAsync(cancellationToken) > 0)
            notifier.Notify();
    }

    /// <summary>
    /// Claims only as much as the channel can take. Claiming ahead would flip rows to
    /// <c>Running</c> while they sat in a buffer — a state the jobs page would display and startup
    /// recovery would have to undo.
    /// </summary>
    private async Task PumpAsync(
        ChannelWriter<Guid> writer,
        int capacity,
        CancellationToken stoppingToken
    )
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var claimed = await ClaimAsync(capacity, stoppingToken);

                if (claimed.Count == 0)
                {
                    // Nothing to do: wait for an enqueue, or look again when the poll elapses.
                    await signal.WaitAsync(_options.PollInterval, stoppingToken);
                    continue;
                }

                notifier.Notify();

                foreach (var id in claimed)
                    await writer.WriteAsync(id, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                // A database blip must not kill the pump for the life of the process.
                logger.LogError(exception, "The job pump failed; retrying after the poll interval");

                await Task.Delay(_options.PollInterval, timeProvider, CancellationToken.None);
            }
        }
    }

    private async Task<IReadOnlyList<Guid>> ClaimAsync(int max, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IJobStore>();

        return await store.ClaimAsync(max, cancellationToken);
    }

    private async Task WorkAsync(ChannelReader<Guid> reader, CancellationToken stoppingToken)
    {
        // Read with None: a worker drains what has already been claimed rather than abandoning it,
        // and the per-job token below is what actually stops work in progress.
        await foreach (var id in reader.ReadAllAsync(CancellationToken.None))
            await ExecuteOneAsync(id, stoppingToken);
    }

    /// <summary>
    /// A scope per job, so each gets a short-lived <c>StriveContext</c> rather than one living as
    /// long as the process — and so the handler resolved here gets that scope's dependencies.
    /// </summary>
    private async Task ExecuteOneAsync(Guid id, CancellationToken stoppingToken)
    {
        await using var scope = services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IJobStore>();

        var job = await store.GetAsync(id, CancellationToken.None);

        if (job is null)
        {
            logger.LogWarning("Claimed job {JobId} no longer exists", id);
            return;
        }

        var progress = new ThrottledProgress(
            value => WriteProgressAsync(id, value),
            _options.ProgressInterval,
            timeProvider
        );

        try
        {
            var handler =
                scope
                    .ServiceProvider.GetServices<IJobHandler>()
                    .FirstOrDefault(candidate => candidate.Kind == job.Kind)
                ?? throw new InvalidOperationException(
                    $"No job handler is registered for the kind '{job.Kind}'."
                );

            await handler.ExecuteAsync(new JobContext(job, progress), stoppingToken);

            await progress.FlushAsync();
            await store.CompleteAsync(id, CancellationToken.None);

            logger.LogInformation("Job {JobId} ({Kind}) succeeded", id, job.Kind);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutdown, not failure. Returned to the queue so the next start resumes it, and
            // without spending the single attempt the retry policy allows.
            await store.ReleaseAsync(id, CancellationToken.None);
            logger.LogInformation("Job {JobId} was interrupted by shutdown and re-queued", id);
        }
        catch (Exception exception)
        {
            await progress.FlushAsync();
            await store.FailAsync(id, exception.Message, CancellationToken.None);
        }
        finally
        {
            notifier.Notify();
        }
    }

    /// <summary>
    /// On its own scope, because progress is written from the throttle's fire-and-forget path
    /// while the job's own scope is in use by the handler — and a <c>DbContext</c> is not safe to
    /// share across the two.
    /// </summary>
    private async Task WriteProgressAsync(Guid id, JobProgress value)
    {
        await using var scope = services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IJobStore>();

        await store.SaveProgressAsync(id, value, CancellationToken.None);
        notifier.Notify();
    }
}
```

- [ ] **Step 7: Wire the registrations**

In `src/Fip.Strive.Application/Registration.cs`, add these usings:

```csharp
using Fip.Strive.Application.Features.Jobs;
using Fip.Strive.Application.Features.Jobs.Services;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
```

add a call inside `AddApplication` after `AddCatalog()`:

```csharp
        services.AddJobs(configuration);
```

and the method itself after `AddCatalog`:

```csharp
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
```

- [ ] **Step 8: Keep the runner out of the host tests**

In `src/Fip.Strive.IntegrationTests/Fixtures/StriveAppFactory.cs`, add to `ConfigureWebHost`:

```csharp
        // These tests boot the host to exercise something else. A live runner polling in the
        // background would be noise at best and a race at worst.
        builder.UseSetting("Jobs:Enabled", "false");
```

- [ ] **Step 9: Run the tests to verify they pass**

```bash
dotnet test src/Fip.Strive.IntegrationTests --filter JobRunnerTests
```

Expected: 6 passed.

- [ ] **Step 10: Run the whole suite**

```bash
dotnet test src/strive.slnx
```

Expected: everything passes.

- [ ] **Step 11: Format and commit**

```bash
dotnet csharpier format src/
git add src/
git commit -m "feat: run jobs on a hosted background service"
```

---

### Task 7: Unpack as a job kind

**Files:**
- Create: `src/Fip.Strive.Application/Features/Import/Services/UnpackJobHandler.cs`
- Create: `src/Fip.Strive.IntegrationTests/Fixtures/UnpackHarness.cs`
- Modify: `src/Fip.Strive.Application/Registration.cs`
- Modify: `src/Fip.Strive.Web/Components/Pages/ImportPage.razor.cs`
- Modify: `src/Fip.Strive.Web/Components/Pages/ImportPage.razor`
- Test: `src/Fip.Strive.IntegrationTests/UnpackJobTests.cs`

**Interfaces:**
- Consumes: `IJobQueue` (Task 3); `IJobHandler`, `JobContext` (Task 2); `IPackageImporter`, `IStagingArea`, `StagedArchive`, `ImportProgress`.
- Produces: `UnpackJobHandler` with `Kind = ComponentId = "unpack"`, `Version = 1`, and the constant `UnpackJobHandler.JobKind`.

The payload is a serialized `StagedArchive` — it already carries exactly `(FileName, Path, Hash, SizeBytes)`, which is everything the handler needs to find the archive again after a restart. A second record with the same four fields would be duplication.

- [ ] **Step 1: Write the harness**

Create `src/Fip.Strive.IntegrationTests/Fixtures/UnpackHarness.cs`:

```csharp
using Fip.Strive.Application.Features.Import.Services;
using Fip.Strive.Application.Features.Import.Services.Contracts;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Fip.Strive.Application.Features.Storage;
using Fip.Strive.Application.Features.Storage.Services;
using Fip.Strive.Application.Features.Storage.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.IntegrationTests.Fixtures;

/// <summary>
/// A job harness whose provider also holds the storage and importer the unpack handler needs,
/// registered with the same lifetimes the app uses.
/// </summary>
public sealed class UnpackHarness : IAsyncDisposable
{
    private readonly string _root;

    private UnpackHarness(string root, StoragePaths paths, string archiveDirectory)
    {
        _root = root;
        Paths = paths;
        ArchiveDirectory = archiveDirectory;
    }

    public JobHarness Jobs { get; private set; } = default!;

    public StoragePaths Paths { get; }

    public string ArchiveDirectory { get; }

    public IStagingArea Staging => Jobs.Resolve<IStagingArea>();

    public static async Task<UnpackHarness> CreateAsync(PostgresFixture fixture)
    {
        var root = Path.Combine(Path.GetTempPath(), "strive-tests", Guid.NewGuid().ToString("n"));

        var paths = new StoragePaths(Path.Combine(root, "data"));
        paths.EnsureCreated();

        var archiveDirectory = Path.Combine(root, "archives");
        Directory.CreateDirectory(archiveDirectory);

        var harness = new UnpackHarness(root, paths, archiveDirectory);

        harness.Jobs = await JobHarness.CreateAsync(
            fixture,
            services =>
            {
                services.AddSingleton(paths);
                services.Configure<StorageOptions>(_ => { });
                services.AddSingleton<IBlobStore, BlobStore>();
                services.AddSingleton<IStagingArea, StagingArea>();
                services.AddScoped<IPackageImporter, PackageImporter>();
                services.AddScoped<IJobHandler, UnpackJobHandler>();
            }
        );

        return harness;
    }

    /// <summary>Stages an archive and queues its unpack job, exactly as the import page does.</summary>
    public async Task<string> StageAndEnqueueAsync(string archivePath)
    {
        await using var source = File.OpenRead(archivePath);
        var staged = await Staging.StageAsync(Path.GetFileName(archivePath), source);

        await Jobs.EnqueueAsync(UnpackJobHandler.JobKind, staged.Hash, staged);

        return staged.Hash;
    }

    public async ValueTask DisposeAsync()
    {
        await Jobs.DisposeAsync();

        try
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }
        catch (IOException)
        {
            // Temp debris only.
        }
    }
}
```

`Configure<StorageOptions>(_ => { })` registers `IOptions<StorageOptions>` at its defaults, which is what `PackageImporter` takes its expansion ceilings from. `JobHarness` already calls `AddOptions()`, so the binding infrastructure is present.

- [ ] **Step 2: Write the failing test**

Create `src/Fip.Strive.IntegrationTests/UnpackJobTests.cs`:

```csharp
using AwesomeAssertions;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.IntegrationTests;

[Collection(PostgresCollection.Name)]
public class UnpackJobTests(PostgresFixture postgres)
{
    [Fact]
    public async Task An_unpack_job_imports_the_archive_and_clears_the_staging_area()
    {
        await using var harness = await UnpackHarness.CreateAsync(postgres);

        var archive = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "export.zip",
            ("activity/day-1.json", "one"),
            ("activity/day-2.json", "two")
        );

        await harness.StageAndEnqueueAsync(archive);
        await harness.Jobs.RunUntilIdleAsync();

        await using var context = harness.Jobs.CreateContext();

        (await context.Jobs.SingleAsync()).State.Should().Be(JobState.Succeeded);
        (await context.ImportPackages.CountAsync()).Should().Be(1);
        (await context.CatalogEntries.CountAsync()).Should().Be(2);
        (await context.PackageFiles.CountAsync()).Should().Be(2);

        Directory
            .EnumerateFiles(harness.Paths.Incoming)
            .Should()
            .BeEmpty("the archive is redundant once its contents are in the blob store");
    }

    [Fact]
    public async Task The_job_is_keyed_by_the_archive_hash()
    {
        await using var harness = await UnpackHarness.CreateAsync(postgres);

        var archive = ZipBuilder.Create(harness.ArchiveDirectory, "export.zip", ("a.json", "one"));

        var hash = await harness.StageAndEnqueueAsync(archive);
        await harness.Jobs.RunUntilIdleAsync();

        await using var context = harness.Jobs.CreateContext();
        var job = await context.Jobs.SingleAsync();

        job.Kind.Should().Be("unpack");
        job.TargetKey.Should().Be(hash);
        job.ComponentId.Should().Be("unpack");
        job.ComponentVersion.Should().Be(1);
    }

    [Fact]
    public async Task Re_uploading_an_archive_reuses_its_work_unit_and_does_no_work()
    {
        await using var harness = await UnpackHarness.CreateAsync(postgres);

        var archive = ZipBuilder.Create(harness.ArchiveDirectory, "export.zip", ("a.json", "one"));

        await harness.StageAndEnqueueAsync(archive);
        await harness.Jobs.RunUntilIdleAsync();

        await harness.StageAndEnqueueAsync(archive);
        await harness.Jobs.RunUntilIdleAsync();

        await using var context = harness.Jobs.CreateContext();

        (await context.Jobs.CountAsync()).Should().Be(1, "one archive is one work unit");
        (await context.ImportPackages.CountAsync()).Should().Be(1);
        (await context.PackageFiles.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task A_failed_unpack_keeps_the_staged_archive_so_a_retry_can_read_it()
    {
        await using var harness = await UnpackHarness.CreateAsync(postgres);

        var archive = ZipBuilder.Create(harness.ArchiveDirectory, "export.zip", ("a.json", "one"));
        await harness.StageAndEnqueueAsync(archive);

        // Corrupt the staged copy so unpacking throws where a real IO fault would.
        var staged = Directory.EnumerateFiles(harness.Paths.Incoming).Single();
        await File.WriteAllTextAsync(staged, "this is not a zip");

        await harness.Jobs.RunUntilIdleAsync();

        await using var context = harness.Jobs.CreateContext();
        var job = await context.Jobs.SingleAsync();

        job.State.Should().Be(JobState.Failed);
        job.Error.Should().NotBeNullOrWhiteSpace();

        File.Exists(staged).Should().BeTrue("a retry has to have something to read");
    }

    [Fact]
    public async Task Unpacking_reports_progress_against_the_job()
    {
        await using var harness = await UnpackHarness.CreateAsync(postgres);

        var files = Enumerable
            .Range(0, 25)
            .Select(index => ($"activity/day-{index}.json", $"payload {index}"))
            .ToArray();

        var archive = ZipBuilder.Create(harness.ArchiveDirectory, "export.zip", files);

        await harness.StageAndEnqueueAsync(archive);
        await harness.Jobs.RunUntilIdleAsync();

        await using var context = harness.Jobs.CreateContext();
        var job = await context.Jobs.SingleAsync();

        job.ProgressCurrent.Should().Be(25);
        job.ProgressTotal.Should().Be(25);
    }
}
```

- [ ] **Step 3: Run the test to verify it fails**

```bash
dotnet test src/Fip.Strive.IntegrationTests --filter UnpackJobTests
```

Expected: compile failure — `UnpackJobHandler` does not exist.

- [ ] **Step 4: Write the handler**

`src/Fip.Strive.Application/Features/Import/Services/UnpackJobHandler.cs`:

```csharp
using System.Text.Json;
using Fip.Strive.Application.Features.Import.Models;
using Fip.Strive.Application.Features.Import.Services.Contracts;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Fip.Strive.Application.Features.Storage.Models;
using Fip.Strive.Application.Features.Storage.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Application.Features.Import.Services;

/// <summary>
/// Unpacking, as a job. Idempotent by way of the importer's duplicate-archive check: a crash
/// between the import's commit and the job's terminal write means the retry finds the package
/// already there and does nothing, which is what keeps a kill mid-run from duplicating work.
/// </summary>
public sealed class UnpackJobHandler(
    IPackageImporter importer,
    IStagingArea staging,
    ILogger<UnpackJobHandler> logger
) : IJobHandler
{
    public const string JobKind = "unpack";

    private static readonly JsonSerializerOptions PayloadOptions = new(JsonSerializerDefaults.Web);

    public string Kind => JobKind;

    public string ComponentId => JobKind;

    /// <summary>
    /// Bumping this would re-unpack every archive, producing byte-identical blobs. It exists for
    /// the shape the later steps need, not because unpacking is expected to change.
    /// </summary>
    public int Version => 1;

    public async Task ExecuteAsync(JobContext context, CancellationToken cancellationToken)
    {
        var archive =
            JsonSerializer.Deserialize<StagedArchive>(
                context.Job.Payload ?? string.Empty,
                PayloadOptions
            )
            ?? throw new InvalidOperationException(
                $"Unpack job {context.Job.Id} carries no usable payload."
            );

        var progress = new Progress<ImportProgress>(update =>
            context.Progress.Report(
                new JobProgress(update.FilesProcessed, update.TotalFiles, update.CurrentPath)
            )
        );

        var result = await importer.ImportAsync(archive, progress, cancellationToken);

        // Only once the import has committed. A failed job keeps its archive so a manual retry has
        // something to read — leftover bytes are a cheaper problem than an unretryable job.
        staging.Discard(archive);

        logger.LogInformation(
            "Unpacked {FileName}: {Outcome}, {FileCount} files",
            archive.FileName,
            result.Outcome,
            result.FileCount
        );
    }
}
```

- [ ] **Step 5: Register the handler**

In `src/Fip.Strive.Application/Registration.cs`, inside `AddCatalog`, after the importer registration:

```csharp
        // Scoped, because it depends on the scoped importer. The registry keeps only its identity;
        // the runner resolves the instance it executes from the job's own scope.
        services.AddScoped<IJobHandler, UnpackJobHandler>();
```

- [ ] **Step 6: Run the tests to verify they pass**

```bash
dotnet test src/Fip.Strive.IntegrationTests --filter UnpackJobTests
```

Expected: 5 passed.

- [ ] **Step 7: Retrofit the import page**

In `src/Fip.Strive.Web/Components/Pages/ImportPage.razor.cs`:

Replace the `IServiceScopeFactory ScopeFactory` injection with:

```csharp
    [Inject]
    private IJobQueue Jobs { get; set; } = default!;
```

Replace `UnpackAsync` with:

```csharp
    private async Task QueueAsync(StagedArchive staged, UploadJob job)
    {
        // Keyed by the archive hash, so re-uploading the same bytes reuses the work unit rather
        // than queueing a second one.
        await Jobs.EnqueueAsync(UnpackJobHandler.JobKind, staged.Hash, staged);

        job.Phase = UploadPhase.Queued;
    }
```

Replace `ImportAsync`'s body — nothing discards the staged archive now, because the handler owns it:

```csharp
    private async Task ImportAsync(IBrowserFile file, CancellationToken cancellationToken)
    {
        var job = new UploadJob(file.Name, file.Size);
        _jobs.Insert(0, job);
        await InvokeAsync(StateHasChanged);

        try
        {
            var staged = await StageAsync(file, job, cancellationToken);
            await QueueAsync(staged, job);

            Snackbar.Add($"{file.Name} queued for unpacking.", Severity.Success);
        }
        catch (OperationCanceledException)
        {
            job.Phase = UploadPhase.Cancelled;
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Upload of {FileName} failed", file.Name);
            job.Phase = UploadPhase.Failed;
            job.Error = exception.Message;
            Snackbar.Add($"{file.Name} failed: {exception.Message}", Severity.Error);
        }
        finally
        {
            await InvokeAsync(StateHasChanged);
        }
    }
```

Delete the `Announce` method, the `Result` property on `UploadJob`, and the `ICatalogReader`/`IPackageImporter` usings if they become unused. Replace `UploadPhase` with:

```csharp
    private enum UploadPhase
    {
        Uploading,
        Queued,
        Cancelled,
        Failed,
    }
```

Update the remaining `UploadJob` members:

```csharp
        public bool ShowsProgress => Phase is UploadPhase.Uploading;

        public double Percent =>
            Phase switch
            {
                UploadPhase.Uploading => SizeBytes == 0 ? 0 : 100d * BytesUploaded / SizeBytes,
                _ => 100,
            };

        public string Detail =>
            Phase switch
            {
                UploadPhase.Uploading => $"Uploading — {ByteSize.Format(BytesUploaded)}",
                UploadPhase.Queued => "Queued for unpacking",
                UploadPhase.Cancelled => "Cancelled",
                UploadPhase.Failed => Error ?? "Failed",
                _ => string.Empty,
            };
```

and the icon and colour switches:

```csharp
    private static string IconFor(UploadJob job) =>
        job.Phase switch
        {
            UploadPhase.Queued => Icons.Material.Outlined.Schedule,
            UploadPhase.Failed => Icons.Material.Outlined.ErrorOutline,
            UploadPhase.Cancelled => Icons.Material.Outlined.Cancel,
            _ => Icons.Material.Outlined.HourglassTop,
        };

    private static Color ColorFor(UploadJob job) =>
        job.Phase switch
        {
            UploadPhase.Queued => Color.Info,
            UploadPhase.Failed => Color.Error,
            UploadPhase.Cancelled => Color.Warning,
            _ => Color.Default,
        };
```

Add the usings `Fip.Strive.Application.Features.Import.Services` (for `UnpackJobHandler.JobKind`) and `Fip.Strive.Application.Features.Jobs.Services.Contracts`.

In `src/Fip.Strive.Web/Components/Pages/ImportPage.razor`, add under the upload control:

```razor
<MudText Typo="Typo.body2" Class="mud-text-secondary mt-2">
    Uploads are unpacked in the background. Watch them on the <MudLink Href="/jobs">Jobs</MudLink>
    page — closing this tab does not stop a run.
</MudText>
```

- [ ] **Step 8: Run the whole suite**

```bash
dotnet test src/strive.slnx
```

Expected: all green. `ImportTests` still passes — it drives `PackageImporter` directly and never went through the page.

- [ ] **Step 9: Format and commit**

```bash
dotnet csharpier format src/
git add src/
git commit -m "feat: unpack archives as jobs instead of on the circuit"
```

---

### Task 8: The jobs page

**Files:**
- Create: `src/Fip.Strive.Application/Features/Jobs/Models/JobViews.cs`
- Create: `src/Fip.Strive.Application/Features/Jobs/Services/Contracts/IJobReader.cs`
- Create: `src/Fip.Strive.Application/Features/Jobs/Services/JobReader.cs`
- Create: `src/Fip.Strive.Web/Components/Pages/JobsPage.razor`
- Create: `src/Fip.Strive.Web/Components/Pages/JobsPage.razor.cs`
- Modify: `src/Fip.Strive.Application/Registration.cs`
- Modify: `src/Fip.Strive.IntegrationTests/Fixtures/JobHarness.cs`
- Modify: `src/Fip.Strive.Web/Components/Layout/NavMenu.razor`
- Test: `src/Fip.Strive.IntegrationTests/JobReaderTests.cs`

**Interfaces:**
- Consumes: `Job`, `JobState` (Task 1); `IJobQueue` (Task 3); `IJobNotifier` (Task 5).
- Produces: `JobRow` with `DurationAt(DateTimeOffset now)` and `Percent`; `JobCounts(int Pending, int Running, int Succeeded, int Failed, int Stale)`; `IJobReader` with `GetCountsAsync` and `GetJobsAsync(int take = 100, …)`.

- [ ] **Step 1: Write the failing test**

Create `src/Fip.Strive.IntegrationTests/JobReaderTests.cs`:

```csharp
using AwesomeAssertions;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Fip.Strive.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.IntegrationTests;

[Collection(PostgresCollection.Name)]
public class JobReaderTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Counts_are_reported_per_state()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);

        await harness.SeedAsync("noop", "a", JobState.Pending);
        await harness.SeedAsync("noop", "b", JobState.Pending);
        await harness.SeedAsync("noop", "c", JobState.Running);
        await harness.SeedAsync("noop", "d", JobState.Failed);

        await using var scope = harness.Scope();
        var counts = await Reader(scope).GetCountsAsync();

        counts.Pending.Should().Be(2);
        counts.Running.Should().Be(1);
        counts.Succeeded.Should().Be(0);
        counts.Failed.Should().Be(1);
    }

    [Fact]
    public async Task Unfinished_jobs_are_listed_before_finished_ones()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);

        await harness.SeedAsync(
            "noop",
            "old-success",
            JobState.Succeeded,
            enqueued: DateTimeOffset.UtcNow.AddHours(-2)
        );
        await harness.SeedAsync(
            "noop",
            "waiting",
            JobState.Pending,
            enqueued: DateTimeOffset.UtcNow.AddHours(-1)
        );

        await using var scope = harness.Scope();
        var jobs = await Reader(scope).GetJobsAsync();

        // What is happening now is what someone opened the page to see.
        jobs.Select(job => job.TargetKey).Should().ContainInOrder("waiting", "old-success");
    }

    [Fact]
    public async Task The_listing_is_capped()
    {
        await using var harness = await JobHarness.CreateAsync(postgres);

        for (var index = 0; index < 30; index++)
            await harness.SeedAsync("noop", $"target-{index}", JobState.Succeeded);

        await using var scope = harness.Scope();

        (await Reader(scope).GetJobsAsync(10)).Should().HaveCount(10);
    }

    private static IJobReader Reader(AsyncServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IJobReader>();
}
```

Register the reader in the harness's provider, next to `IJobStore`:

```csharp
        services.AddScoped<IJobReader, JobReader>();
```

- [ ] **Step 2: Run the test to verify it fails**

```bash
dotnet test src/Fip.Strive.IntegrationTests --filter JobReaderTests
```

Expected: compile failure — `IJobReader` and `JobReader` do not exist.

- [ ] **Step 3: Write the views and reader**

`src/Fip.Strive.Application/Features/Jobs/Models/JobViews.cs`:

```csharp
namespace Fip.Strive.Application.Features.Jobs.Models;

public sealed record JobRow(
    Guid Id,
    string Kind,
    string TargetKey,
    string ComponentId,
    int ComponentVersion,
    JobState State,
    int Attempts,
    string? Error,
    int? ProgressCurrent,
    int? ProgressTotal,
    string? ProgressNote,
    DateTimeOffset EnqueuedUtc,
    DateTimeOffset? StartedUtc,
    DateTimeOffset? FinishedUtc
)
{
    /// <summary>
    /// Takes the current time rather than reading a clock, so a running job's elapsed time comes
    /// from the caller's <c>TimeProvider</c> like every other timestamp in the app.
    /// </summary>
    public TimeSpan? DurationAt(DateTimeOffset now) =>
        StartedUtc is null ? null : (FinishedUtc ?? now) - StartedUtc;

    public double? Percent =>
        ProgressTotal is null or 0 ? null : 100d * (ProgressCurrent ?? 0) / ProgressTotal.Value;
}

public sealed record JobCounts(int Pending, int Running, int Succeeded, int Failed, int Stale);
```

`src/Fip.Strive.Application/Features/Jobs/Services/Contracts/IJobReader.cs`:

```csharp
using Fip.Strive.Application.Features.Jobs.Models;

namespace Fip.Strive.Application.Features.Jobs.Services.Contracts;

public interface IJobReader
{
    Task<JobCounts> GetCountsAsync(CancellationToken cancellationToken = default);

    /// <summary>Unfinished work first, then the most recent finished work.</summary>
    Task<IReadOnlyList<JobRow>> GetJobsAsync(
        int take = 100,
        CancellationToken cancellationToken = default
    );
}
```

`src/Fip.Strive.Application/Features/Jobs/Services/JobReader.cs`:

```csharp
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Fip.Strive.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Application.Features.Jobs.Services;

public sealed class JobReader(StriveContext context) : IJobReader
{
    public async Task<JobCounts> GetCountsAsync(CancellationToken cancellationToken = default)
    {
        // One round trip rather than five: every open jobs page re-reads this on every change.
        var counts = await context
            .Jobs.AsNoTracking()
            .GroupBy(job => job.State)
            .Select(group => new { State = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.State, row => row.Count, cancellationToken);

        return new JobCounts(
            counts.GetValueOrDefault(JobState.Pending),
            counts.GetValueOrDefault(JobState.Running),
            counts.GetValueOrDefault(JobState.Succeeded),
            counts.GetValueOrDefault(JobState.Failed),
            counts.GetValueOrDefault(JobState.Stale)
        );
    }

    public async Task<IReadOnlyList<JobRow>> GetJobsAsync(
        int take = 100,
        CancellationToken cancellationToken = default
    ) =>
        await context
            .Jobs.AsNoTracking()
            // Running, then pending, then everything finished — someone opening this page is
            // looking for what is happening now, not for last week's successes.
            .OrderBy(job =>
                job.State == JobState.Running ? 0
                : job.State == JobState.Pending ? 1
                : 2
            )
            .ThenByDescending(job => job.FinishedUtc ?? job.StartedUtc ?? job.EnqueuedUtc)
            .Take(take)
            .Select(job => new JobRow(
                job.Id,
                job.Kind,
                job.TargetKey,
                job.ComponentId,
                job.ComponentVersion,
                job.State,
                job.Attempts,
                job.Error,
                job.ProgressCurrent,
                job.ProgressTotal,
                job.ProgressNote,
                job.EnqueuedUtc,
                job.StartedUtc,
                job.FinishedUtc
            ))
            .ToListAsync(cancellationToken);
}
```

- [ ] **Step 4: Register the reader in the app**

In `AddJobs` in `src/Fip.Strive.Application/Registration.cs`, next to the other scoped services:

```csharp
        services.AddScoped<IJobReader, JobReader>();
```

- [ ] **Step 5: Run the tests to verify they pass**

```bash
dotnet test src/Fip.Strive.IntegrationTests --filter JobReaderTests
```

Expected: 3 passed.

- [ ] **Step 6: Write the page**

`src/Fip.Strive.Web/Components/Pages/JobsPage.razor`:

```razor
@page "/jobs"
@using Fip.Strive.Application.Features.Jobs.Models

<PageTitle>Jobs — Strive</PageTitle>

<Headline Text="Jobs"/>

@if (_jobs is null)
{
    <MudProgressLinear Indeterminate="true"/>
}
else
{
    <MudStack Row="true" Spacing="2" Class="mb-4">
        <MudChip T="string" Icon="@Icons.Material.Outlined.Schedule" Color="Color.Default">
            @_counts.Pending pending
        </MudChip>
        <MudChip T="string" Icon="@Icons.Material.Outlined.PlayArrow" Color="Color.Info">
            @_counts.Running running
        </MudChip>
        <MudChip T="string" Icon="@Icons.Material.Outlined.CheckCircle" Color="Color.Success">
            @_counts.Succeeded succeeded
        </MudChip>
        <MudChip T="string" Icon="@Icons.Material.Outlined.ErrorOutline" Color="Color.Error">
            @_counts.Failed failed
        </MudChip>
    </MudStack>

    @if (_jobs.Count == 0)
    {
        <MudAlert Severity="Severity.Info">
            Nothing queued yet. <MudLink Href="/import">Upload a takeout archive</MudLink> to get started.
        </MudAlert>
    }
    else
    {
        <MudTable Items="_jobs" Dense="true" Hover="true" Breakpoint="Breakpoint.Sm">
            <HeaderContent>
                <MudTh>Kind</MudTh>
                <MudTh>Target</MudTh>
                <MudTh>State</MudTh>
                <MudTh>Progress</MudTh>
                <MudTh Style="text-align: right">Duration</MudTh>
                <MudTh></MudTh>
            </HeaderContent>
            <RowTemplate>
                <MudTd DataLabel="Kind">
                    <MudStack Spacing="0">
                        <MudText Typo="Typo.body2">@context.Kind</MudText>
                        <MudText Typo="Typo.caption" Class="mud-text-secondary">
                            @context.ComponentId v@context.ComponentVersion
                        </MudText>
                    </MudStack>
                </MudTd>
                <MudTd DataLabel="Target"><code>@Shorten(context.TargetKey)</code></MudTd>
                <MudTd DataLabel="State">
                    <MudStack Spacing="0">
                        <MudChip T="string" Size="Size.Small" Color="@ColorFor(context.State)">
                            @context.State
                        </MudChip>
                        @if (context.Error is not null)
                        {
                            <MudText Typo="Typo.caption" Color="Color.Error">@context.Error</MudText>
                        }
                    </MudStack>
                </MudTd>
                <MudTd DataLabel="Progress" Style="min-width: 160px">
                    @if (context.Percent is { } percent)
                    {
                        <MudTooltip Text="@context.ProgressNote">
                            <MudProgressLinear Value="percent" Color="Color.Primary"/>
                        </MudTooltip>
                        <MudText Typo="Typo.caption" Class="mud-text-secondary">
                            @context.ProgressCurrent of @context.ProgressTotal
                        </MudText>
                    }
                </MudTd>
                <MudTd DataLabel="Duration" Style="text-align: right">@FormatDuration(context)</MudTd>
                <MudTd>
                    @if (context.State == JobState.Failed)
                    {
                        <MudButton Size="Size.Small"
                                   Variant="Variant.Outlined"
                                   StartIcon="@Icons.Material.Outlined.Refresh"
                                   OnClick="@(() => RetryAsync(context))">
                            Retry
                        </MudButton>
                    }
                </MudTd>
            </RowTemplate>
        </MudTable>
    }
}
```

`src/Fip.Strive.Web/Components/Pages/JobsPage.razor.cs`:

```csharp
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fip.Strive.Web.Components.Pages;

public partial class JobsPage : IDisposable
{
    /// <summary>
    /// A busy run notifies far more often than a screen can usefully change. The same bound the
    /// import page puts on its upload progress.
    /// </summary>
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMilliseconds(200);

    [Inject]
    private IJobReader Reader { get; set; } = default!;

    [Inject]
    private IJobQueue Queue { get; set; } = default!;

    [Inject]
    private IJobNotifier Notifier { get; set; } = default!;

    [Inject]
    private TimeProvider Clock { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private IDisposable? _subscription;
    private IReadOnlyList<JobRow>? _jobs;
    private JobCounts _counts = new(0, 0, 0, 0, 0);
    private DateTimeOffset _lastRefresh = DateTimeOffset.MinValue;

    protected override async Task OnInitializedAsync()
    {
        await RefreshAsync();

        // Subscribed after the first read, so a notification arriving mid-load cannot race it.
        _subscription = Notifier.Subscribe(RequestRefresh);
    }

    private void RequestRefresh()
    {
        var now = Clock.GetUtcNow();

        if (now - _lastRefresh < RefreshInterval)
            return;

        _lastRefresh = now;
        _ = InvokeAsync(RefreshAsync);
    }

    private async Task RefreshAsync()
    {
        // Re-read rather than trust the notification's payload: there is none, deliberately.
        _counts = await Reader.GetCountsAsync();
        _jobs = await Reader.GetJobsAsync();

        StateHasChanged();
    }

    private async Task RetryAsync(JobRow job)
    {
        // No payload: the queue keeps the stored one, which for an unpack job is the only record
        // of where its archive is.
        await Queue.EnqueueAsync(job.Kind, job.TargetKey);
        Snackbar.Add($"{job.Kind} job re-queued.", Severity.Info);

        await RefreshAsync();
    }

    private string FormatDuration(JobRow job) =>
        job.DurationAt(Clock.GetUtcNow()) is { } duration
            ? $"{duration.TotalSeconds:N1}s"
            : "—";

    private static Color ColorFor(JobState state) =>
        state switch
        {
            JobState.Running => Color.Info,
            JobState.Succeeded => Color.Success,
            JobState.Failed => Color.Error,
            JobState.Stale => Color.Warning,
            _ => Color.Default,
        };

    private static string Shorten(string targetKey) =>
        targetKey.Length <= 16 ? targetKey : targetKey[..16];

    public void Dispose() => _subscription?.Dispose();
}
```

- [ ] **Step 7: Add the nav entry**

In `src/Fip.Strive.Web/Components/Layout/NavMenu.razor`, after the Import link:

```razor
    <MudNavLink Href="/jobs" Match="NavLinkMatch.Prefix" Icon="@Icons.Material.Outlined.Bolt">
        Jobs
    </MudNavLink>
```

- [ ] **Step 8: Run the whole suite**

```bash
dotnet test src/strive.slnx
```

Expected: all green.

- [ ] **Step 9: Format and commit**

```bash
dotnet csharpier format src/
git add src/
git commit -m "feat: add the live jobs page"
```

---

### Task 9: The done criterion, end to end

**Files:**
- Modify: `src/Fip.Strive.IntegrationTests/Fixtures/JobHarness.cs`
- Test: `src/Fip.Strive.IntegrationTests/JobRecoveryTests.cs`

**Interfaces:**
- Consumes: everything from Tasks 1–8.
- Produces: `JobHarness.KillMidRunAsync()`.

- [ ] **Step 1: Add a killable runner to the harness**

Add to `src/Fip.Strive.IntegrationTests/Fixtures/JobHarness.cs`:

```csharp
    /// <summary>
    /// Starts a runner, waits until a job is actually running, then drops it without a graceful
    /// stop and leaves the row marked <c>Running</c> — which is the state a killed process leaves
    /// behind, and the state startup recovery exists to clean up.
    /// </summary>
    public async Task KillMidRunAsync(TimeSpan? timeout = null)
    {
        var runner = CreateRunner();
        await runner.StartAsync(CancellationToken.None);

        var deadline = DateTimeOffset.UtcNow + (timeout ?? TimeSpan.FromSeconds(30));
        var sawRunning = false;

        while (DateTimeOffset.UtcNow < deadline)
        {
            await using var context = CreateContext();

            if (await context.Jobs.AnyAsync(job => job.State == JobState.Running))
            {
                sawRunning = true;
                break;
            }

            await Task.Delay(10);
        }

        Assert.True(sawRunning, "no job started running, so nothing was interrupted");

        // Dispose without StopAsync: cancels, does not drain.
        runner.Dispose();

        // Let the cancellation settle before overwriting the row, so the worker's own Release
        // cannot land after this and undo it.
        await Task.Delay(500);

        await using var writer = CreateContext();
        await writer
            .Jobs.Where(job => job.State != JobState.Succeeded)
            .ExecuteUpdateAsync(setters =>
                setters
                    .SetProperty(job => job.State, JobState.Running)
                    .SetProperty(job => job.StartedUtc, DateTimeOffset.UtcNow)
            );
    }
```

An in-process runner cancels cooperatively, so its worker gets to release the job — which a killed process would not. Forcing the row back to `Running` is what makes this a kill rather than a graceful stop, and it is the whole point of the test.

- [ ] **Step 2: Write the failing test**

Create `src/Fip.Strive.IntegrationTests/JobRecoveryTests.cs`:

```csharp
using AwesomeAssertions;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.IntegrationTests;

/// <summary>
/// Step 2's done criterion, mechanised: a large unpack killed mid-run resumes on restart and
/// completes with no duplicate or lost work.
/// </summary>
[Collection(PostgresCollection.Name)]
public class JobRecoveryTests(PostgresFixture postgres)
{
    [Fact]
    public async Task A_run_killed_mid_unpack_resumes_and_completes_exactly_once()
    {
        await using var harness = await UnpackHarness.CreateAsync(postgres);

        // Large enough that the kill lands mid-unpack rather than after it.
        var files = Enumerable
            .Range(0, 400)
            .Select(index => ($"activity/day-{index}.json", $"payload number {index}"))
            .ToArray();

        var archive = ZipBuilder.Create(harness.ArchiveDirectory, "export.zip", files);
        await harness.StageAndEnqueueAsync(archive);

        await harness.Jobs.KillMidRunAsync();

        await using (var context = harness.Jobs.CreateContext())
        {
            (await context.Jobs.SingleAsync())
                .State.Should()
                .Be(JobState.Running, "a killed process leaves its claim behind");
        }

        // A fresh runner: startup recovery has to find the claim and finish the work.
        await harness.Jobs.RunUntilIdleAsync();

        await using var reader = harness.Jobs.CreateContext();

        (await reader.Jobs.SingleAsync()).State.Should().Be(JobState.Succeeded);

        (await reader.ImportPackages.CountAsync())
            .Should()
            .Be(1, "a resumed run must not import the archive twice");

        (await reader.PackageFiles.CountAsync()).Should().Be(400, "no file may be lost");

        (await reader.CatalogEntries.CountAsync())
            .Should()
            .Be(400, "every payload is distinct, and none may be duplicated");
    }

    [Fact]
    public async Task Nothing_is_left_claimed_after_a_restart()
    {
        await using var harness = await UnpackHarness.CreateAsync(postgres);

        var files = Enumerable
            .Range(0, 200)
            .Select(index => ($"a/{index}.json", $"payload {index}"))
            .ToArray();

        var archive = ZipBuilder.Create(harness.ArchiveDirectory, "export.zip", files);
        await harness.StageAndEnqueueAsync(archive);

        await harness.Jobs.KillMidRunAsync();
        await harness.Jobs.RunUntilIdleAsync();

        await using var reader = harness.Jobs.CreateContext();

        (await reader.Jobs.CountAsync(job => job.State == JobState.Running))
            .Should()
            .Be(0, "startup recovery re-queues anything a dead process left claimed");
    }
}
```

- [ ] **Step 3: Run the tests**

```bash
dotnet test src/Fip.Strive.IntegrationTests --filter JobRecoveryTests
```

Expected: 2 passed. If `KillMidRunAsync` consistently asserts that no job started running, raise the file count until the unpack is slow enough to catch — the test is worthless if it never actually interrupts anything.

- [ ] **Step 4: Format and commit**

```bash
dotnet csharpier format src/
git add src/
git commit -m "test: prove a killed run resumes without duplicating work"
```

---

### Task 10: Documentation

**Files:**
- Modify: `Readme.md`
- Modify: `docs/roadmap.md`
- Modify: `docs/roadmap/step-2-job-engine.md`

**Interfaces:**
- Consumes: everything from Tasks 1–9.
- Produces: nothing.

- [ ] **Step 1: Document the settings**

In `Readme.md`, add to the Configuration table after `Storage:MaxTotalUncompressedBytes`:

```markdown
| `Jobs:Enabled` | `Jobs__Enabled` | `true` | Whether the background job runner starts. Off only for tests that drive jobs themselves. |
| `Jobs:Parallelism` | `Jobs__Parallelism` | processors, max 8 | Concurrent job workers. |
| `Jobs:PollInterval` | `Jobs__PollInterval` | `00:00:05` | How long the pump waits for a signal before looking anyway. |
| `Jobs:ProgressInterval` | `Jobs__ProgressInterval` | `00:00:00.5` | Floor between persisted progress writes. |
```

Add beneath the expansion-limit note:

```markdown
Unpacking runs as a background job, not on the request that uploaded the archive. Closing the
browser tab during a run has no effect on it, and a run interrupted by a restart resumes from the
job table. Watch progress on `/jobs`.
```

- [ ] **Step 2: Tick the roadmap**

In `docs/roadmap.md`, change step 2's status cell from `☐` to `☑`.

- [ ] **Step 3: Record the result**

In `docs/roadmap/step-2-job-engine.md`, tick every task except the staleness one, which becomes:

```markdown
- [~] Staleness mechanic: component registry with versions **(registry and version stamping only;
  the invalidation sweep moves to step 3)**
```

Then append a `## Result` section in the style step 1 uses. Write it from what the code actually
does, not from this plan. It must cover:

- the `jobs` table, one row per `(kind, target_key)` work unit, and what that costs — no per-run history
- Postgres as the queue with a `SKIP LOCKED` claim, and why the channel sits downstream of it
- startup recovery, and that an interruption does not spend an attempt
- one attempt then park, with manual retry from the page
- the deferred sweep, and that `Stale` is already recovered even though nothing sets it
- the known limit: a permanently failed unpack leaves its archive in `incoming/`, and nothing cleans it up

- [ ] **Step 4: Commit**

```bash
git add Readme.md docs/
git commit -m "docs: record the job engine and tick step 2"
```

---

## Verification

After Task 10, confirm the whole thing from a clean state:

```bash
dotnet csharpier check src/
```

```bash
dotnet test src/strive.slnx
```

Then run it for real and exercise the done criterion by hand, which is what the roadmap actually asks for:

```bash
dotnet run --project src/Fip.Strive.AppHost
```

Upload a genuinely large takeout on `/import`, watch `/jobs` show it running, close the browser tab and reopen it — the run continues. Then kill the app mid-run and start it again: startup recovery re-queues the job and it completes, and `/packages` shows exactly one package with the right file count.
