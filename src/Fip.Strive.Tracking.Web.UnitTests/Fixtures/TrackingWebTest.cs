using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace Fip.Strive.Tracking.Web.UnitTests.Fixtures;

/// <summary>
/// A private data directory per test, and the hosts built against it torn down afterwards. xunit
/// builds one instance per test method, so no two tests share a database file.
/// </summary>
public abstract class TrackingWebTest : IAsyncLifetime
{
    private readonly List<TrackingAppFactory> _factories = [];

    private readonly string _dataDirectory = Path.Combine(
        Path.GetTempPath(),
        "tracking-web-tests",
        Guid.NewGuid().ToString("n")
    );

    /// <summary>
    /// Redirects are not followed: a sign-in redirect and the page it lands on are different
    /// assertions, and following them would turn a 302 into whatever it points at.
    /// </summary>
    /// <param name="apiKey">Null configures no key at all, which turns the pull API off.</param>
    protected HttpClient Client(string? apiKey = TrackingAppFactory.ApiKey)
    {
        var factory = new TrackingAppFactory(_dataDirectory, apiKey);
        _factories.Add(factory);

        return factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }
        );
    }

    /// <summary>
    /// A host running as <c>Production</c>, which is the only way to reach the environment-gated
    /// half of the pipeline. One client per test: the login rate limiter keeps its buckets on the
    /// host, so a second host would silently reset them mid-test.
    /// </summary>
    /// <param name="host">
    /// Becomes the request's <c>Host</c>. HSTS skips <c>localhost</c> by default, so anything
    /// asserting on that header has to arrive as something else.
    /// </param>
    protected HttpClient ProductionClient(
        IReadOnlyDictionary<string, string>? settings = null,
        string host = "tracker.example"
    )
    {
        var factory = new TrackingAppFactory(
            _dataDirectory,
            TrackingAppFactory.ApiKey,
            "Production",
            settings
        );

        _factories.Add(factory);

        return factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri($"http://{host}/"),
            }
        );
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var factory in _factories)
            await factory.DisposeAsync();

        // Pooled connections keep the file open on Windows, and an open file cannot be deleted.
        SqliteConnection.ClearAllPools();

        try
        {
            if (Directory.Exists(_dataDirectory))
                Directory.Delete(_dataDirectory, recursive: true);
        }
        catch (IOException)
        {
            // Temp debris only.
        }
    }
}
