using System.Net;
using System.Text.Json;
using AwesomeAssertions;
using Fip.Strive.IntegrationTests.Fixtures;

namespace Fip.Strive.IntegrationTests;

/// <summary>
/// The app boots end to end: migrations apply, the health endpoint reports, and every page of the
/// L0 surface renders inside the MudBlazor shell.
/// </summary>
[Collection(PostgresCollection.Name)]
public class ShellTests(PostgresFixture postgres) : IAsyncLifetime
{
    private string _dataDirectory = string.Empty;
    private StriveAppFactory _factory = null!;

    public async Task InitializeAsync()
    {
        _dataDirectory = Path.Combine(
            Path.GetTempPath(),
            "strive-tests",
            Guid.NewGuid().ToString("n")
        );

        _factory = new StriveAppFactory(await postgres.CreateDatabaseAsync(), _dataDirectory);
    }

    [Fact]
    public async Task Health_endpoint_reports_healthy()
    {
        var response = await _factory.CreateClient().GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.GetProperty("status").GetString().Should().Be("Healthy");
    }

    [Fact]
    public async Task Startup_creates_the_data_directory_layout()
    {
        // Force the host to build.
        _ = _factory.CreateClient();

        Directory.Exists(Path.Combine(_dataDirectory, "blobs")).Should().BeTrue();
        Directory.Exists(Path.Combine(_dataDirectory, "incoming")).Should().BeTrue();
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/import")]
    [InlineData("/packages")]
    [InlineData("/catalog")]
    public async Task Pages_render_inside_the_shell(string path)
    {
        var response = await _factory.CreateClient().GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("mud-appbar", "the app bar must be part of the shell");
        html.Should().Contain("mud-drawer", "the nav menu drawer must be part of the shell");
    }

    [Fact]
    public async Task Packages_page_says_so_when_nothing_is_imported()
    {
        var html = await _factory.CreateClient().GetStringAsync("/packages");

        html.Should().Contain("Nothing imported yet");
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();

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
