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
