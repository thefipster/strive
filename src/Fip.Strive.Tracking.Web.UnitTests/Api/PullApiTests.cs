using System.Net;
using System.Text.Json;
using AwesomeAssertions;
using Fip.Strive.Tracking.Web.UnitTests.Fixtures;

namespace Fip.Strive.Tracking.Web.UnitTests.Api;

/// <summary>
/// The read-only side the homelab pulls with. It is keyed rather than cookied, and an instance
/// with no key configured is meant to have no API at all rather than one that refuses everything.
/// </summary>
public class PullApiTests : TrackingWebTest
{
    private const string HeaderName = "X-Api-Key";

    [Theory]
    [InlineData("/api/v1/trackers")]
    [InlineData("/api/v1/events")]
    public async Task Refuse_a_call_without_the_key(string path)
    {
        var response = await Client().GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refuses_a_wrong_key()
    {
        var client = Client();
        client.DefaultRequestHeaders.Add(HeaderName, "not the configured key at all, but long");

        var response = await client.GetAsync("/api/v1/trackers");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Answers_with_the_key()
    {
        var client = Client();
        client.DefaultRequestHeaders.Add(HeaderName, TrackingAppFactory.ApiKey);

        var response = await client.GetAsync("/api/v1/trackers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task Is_not_served_when_no_key_is_configured()
    {
        var response = await Client(apiKey: null).GetAsync("/api/v1/trackers");

        // Asserted as "not answered" rather than as a specific code: the endpoint is never mapped,
        // so what comes back is whatever routing does with an unknown path, and pinning that would
        // be testing the framework rather than the rule.
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }
}
