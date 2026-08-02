using System.Net;
using System.Text.Json;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Fip.Strive.Web.UnitTests;

/// <summary>
/// Smoke tests for the app shell: the host boots, the health endpoint reports, and the
/// MudBlazor layout renders. Every later roadmap step builds on top of these three facts.
/// </summary>
public class ShellTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Health_endpoint_reports_healthy()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.GetProperty("status").GetString().Should().Be("Healthy");
    }

    [Fact]
    public async Task Home_page_renders_the_shell()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("<title>Strive</title>");
        html.Should().Contain("mud-appbar", "the MudBlazor app bar must be part of the shell");
        html.Should().Contain("mud-drawer", "the nav menu drawer must be part of the shell");
    }
}
