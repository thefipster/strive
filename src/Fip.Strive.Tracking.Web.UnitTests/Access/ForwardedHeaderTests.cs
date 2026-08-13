using System.Net;
using AwesomeAssertions;
using Fip.Strive.Tracking.Web.UnitTests.Fixtures;

namespace Fip.Strive.Tracking.Web.UnitTests.Access;

/// <summary>
/// The container speaks plain HTTP behind a proxy that terminates TLS, so the scheme and the
/// caller's address arrive only as headers. Whether those are honoured decides two things that
/// otherwise fail silently: whether HSTS is ever sent, and whether the login rate limit applies per
/// caller or to everyone at once.
/// </summary>
public class ForwardedHeaderTests : TrackingWebTest
{
    private static readonly Dictionary<string, string> BehindProxy = new()
    {
        ["Proxy:Enabled"] = "true",
    };

    [Fact]
    public async Task Hsts_is_not_sent_while_the_proxy_is_untrusted()
    {
        var response = await GetAsProxiedAsync(ProductionClient(), "198.51.100.7");

        // The header is added only when the request looks secure, and without forwarded headers it
        // never does — so the app quietly serves an internet-facing site with no HSTS at all.
        response.Headers.Contains("Strict-Transport-Security").Should().BeFalse();
    }

    [Fact]
    public async Task Hsts_is_sent_once_forwarded_headers_are_trusted()
    {
        var response = await GetAsProxiedAsync(ProductionClient(BehindProxy), "198.51.100.7");

        response
            .Headers.Contains("Strict-Transport-Security")
            .Should()
            .BeTrue("the proxy reported the request arrived over HTTPS");
    }

    [Fact]
    public async Task The_login_limit_is_shared_by_everyone_while_the_proxy_is_untrusted()
    {
        var client = ProductionClient();

        // Ten distinct callers, which is the whole per-window allowance.
        for (var caller = 1; caller <= 10; caller++)
            await PostBadPasswordAsync(client, $"203.0.113.{caller}");

        var eleventh = await PostBadPasswordAsync(client, "203.0.113.11");

        // Every caller landed in the proxy's single bucket, so one attacker exhausts the allowance
        // and locks the real user out.
        eleventh.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task The_login_limit_is_per_caller_once_forwarded_headers_are_trusted()
    {
        var client = ProductionClient(BehindProxy);

        for (var attempt = 1; attempt <= 10; attempt++)
            await PostBadPasswordAsync(client, "203.0.113.1");

        var sameCaller = await PostBadPasswordAsync(client, "203.0.113.1");
        var otherCaller = await PostBadPasswordAsync(client, "203.0.113.2");

        sameCaller.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        otherCaller
            .StatusCode.Should()
            .Be(HttpStatusCode.Found, "a different caller has its own allowance");
    }

    [Fact]
    public async Task A_named_proxy_is_honoured()
    {
        var client = ProductionClient(
            new Dictionary<string, string>
            {
                ["Proxy:Enabled"] = "true",
                // The in-memory test server reports the caller as ::1.
                ["Proxy:KnownProxies:0"] = "::1",
            }
        );

        var response = await GetAsProxiedAsync(client, "198.51.100.7");

        response.Headers.Contains("Strict-Transport-Security").Should().BeTrue();
    }

    [Fact]
    public void A_mistyped_proxy_address_refuses_to_start()
    {
        var client = () =>
            ProductionClient(
                new Dictionary<string, string>
                {
                    ["Proxy:Enabled"] = "true",
                    ["Proxy:KnownProxies:0"] = "10.0.0.999",
                }
            );

        // Silently dropping it would be the worst outcome: this list decides who may rewrite a
        // caller's address, so a typo would narrow the trust with nothing to say it had.
        client.Should().Throw<InvalidOperationException>().WithMessage("*10.0.0.999*");
    }

    [Fact]
    public void A_mistyped_network_refuses_to_start()
    {
        var client = () =>
            ProductionClient(
                new Dictionary<string, string>
                {
                    ["Proxy:Enabled"] = "true",
                    ["Proxy:KnownNetworks:0"] = "10.0.0.0/not-a-prefix",
                }
            );

        client.Should().Throw<InvalidOperationException>().WithMessage("*10.0.0.0/not-a-prefix*");
    }

    private static async Task<HttpResponseMessage> GetAsProxiedAsync(
        HttpClient client,
        string caller
    )
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/login.html");
        AddProxyHeaders(request, caller);

        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> PostBadPasswordAsync(
        HttpClient client,
        string caller
    )
    {
        // Disposing the request disposes the content it owns, so the body needs no `using` of
        // its own. Safe here because the response is fully buffered by the time SendAsync returns.
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/login")
        {
            Content = new FormUrlEncodedContent([
                new KeyValuePair<string, string>("password", "not the password"),
            ]),
        };

        AddProxyHeaders(request, caller);

        return await client.SendAsync(request);
    }

    private static void AddProxyHeaders(HttpRequestMessage request, string caller)
    {
        request.Headers.Add("X-Forwarded-For", caller);
        request.Headers.Add("X-Forwarded-Proto", "https");
    }
}
