using System.Net;
using System.Text.Json;
using AwesomeAssertions;
using Fip.Strive.Tracking.Web.UnitTests.Fixtures;

namespace Fip.Strive.Tracking.Web.UnitTests.Health;

/// <summary>
/// The endpoint is mapped as a route handler specifically so the API key filter runs against it —
/// filters are only built for route handler delegates, so the obvious spellings
/// (<c>AddEndpointFilter</c> on <c>MapHealthChecks</c>, or a handler taking nothing but an
/// <c>HttpContext</c>) compile and then never invoke the filter. These tests exist because that
/// failure mode is invisible: the endpoint keeps answering, just to everyone.
/// </summary>
public class HealthEndpointTests : TrackingWebTest
{
    private const string HeaderName = "X-Api-Key";

    [Fact]
    public async Task Refuses_a_call_without_the_api_key()
    {
        var response = await Client().GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refuses_a_wrong_api_key()
    {
        var client = Client();
        client.DefaultRequestHeaders.Add(HeaderName, "not the configured key at all, but long");

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Reports_on_the_database_with_the_api_key()
    {
        var client = Client();
        client.DefaultRequestHeaders.Add(HeaderName, TrackingAppFactory.ApiKey);

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.GetProperty("status").GetString().Should().Be("Healthy");

        // The whole point of the endpoint: a named check that queried something, rather than an
        // empty result set reported as healthy because nothing was registered.
        var results = document.RootElement.GetProperty("results").EnumerateArray().ToList();

        results.Should().ContainSingle("the database check is the readiness signal");
        results[0].GetProperty("name").GetString().Should().Be("sqlite");
        results[0].GetProperty("status").GetString().Should().Be("Healthy");
    }

    [Fact]
    public async Task Stays_public_when_no_api_key_is_configured()
    {
        // There would be nothing to authenticate with, and a probe that could only ever be
        // refused reads as a permanently unhealthy container.
        var response = await Client(apiKey: null).GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
