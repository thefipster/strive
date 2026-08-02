using Fip.Strive.Application.Features.Catalog.Services;
using Fip.Strive.Application.Features.Catalog.Services.Contracts;
using Fip.Strive.Application.Features.Import.Models;
using Fip.Strive.Application.Features.Import.Services;
using Fip.Strive.Application.Features.Storage;
using Fip.Strive.Application.Features.Storage.Services;
using Fip.Strive.Application.Features.Storage.Services.Contracts;
using Fip.Strive.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Fip.Strive.IntegrationTests.Fixtures;

/// <summary>
/// A private database plus a private data directory, wired the way the app wires them. Contexts
/// are created per call so assertions read from the database rather than a change tracker.
/// </summary>
public sealed class ImportHarness : IAsyncDisposable
{
    private readonly string _connectionString;
    private readonly string _root;

    private ImportHarness(string connectionString)
    {
        _connectionString = connectionString;
        _root = Path.Combine(Path.GetTempPath(), "strive-tests", Guid.NewGuid().ToString("n"));

        Paths = new StoragePaths(Path.Combine(_root, "data"));
        Paths.EnsureCreated();

        ArchiveDirectory = Path.Combine(_root, "archives");
        Directory.CreateDirectory(ArchiveDirectory);

        Blobs = new BlobStore(Paths);
        Staging = new StagingArea(Paths, NullLogger<StagingArea>.Instance);
    }

    public StoragePaths Paths { get; }

    public string ArchiveDirectory { get; }

    public IBlobStore Blobs { get; }

    public IStagingArea Staging { get; }

    public static async Task<ImportHarness> CreateAsync(PostgresFixture fixture)
    {
        var harness = new ImportHarness(await fixture.CreateDatabaseAsync());

        await using var context = harness.CreateContext();
        await context.Database.MigrateAsync();

        return harness;
    }

    public StriveContext CreateContext() =>
        new(new DbContextOptionsBuilder<StriveContext>().UseNpgsql(_connectionString).Options);

    public ICatalogReader CreateCatalogReader(StriveContext context) => new CatalogReader(context);

    /// <summary>Stages and imports an archive exactly as the import page does.</summary>
    public async Task<ImportResult> ImportAsync(string archivePath)
    {
        await using var source = File.OpenRead(archivePath);
        var staged = await Staging.StageAsync(Path.GetFileName(archivePath), source);

        try
        {
            await using var context = CreateContext();
            var importer = new PackageImporter(
                context,
                Blobs,
                TimeProvider.System,
                NullLogger<PackageImporter>.Instance
            );

            return await importer.ImportAsync(staged);
        }
        finally
        {
            Staging.Discard(staged);
        }
    }

    public int CountBlobs() =>
        Directory
            .EnumerateFiles(Paths.Blobs, "*", SearchOption.AllDirectories)
            .Count(path => !path.Contains(".tmp", StringComparison.Ordinal));

    public async ValueTask DisposeAsync()
    {
        await using (var context = CreateContext())
        {
            await context.Database.EnsureDeletedAsync();
        }

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
