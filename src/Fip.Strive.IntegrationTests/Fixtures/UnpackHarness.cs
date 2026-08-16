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

                // Registers IOptions<StorageOptions> at its defaults, which is where the importer
                // takes its expansion ceilings from.
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
