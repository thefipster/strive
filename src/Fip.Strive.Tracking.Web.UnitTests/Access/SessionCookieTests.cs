using AwesomeAssertions;
using Fip.Strive.Tracking.Web.UnitTests.Fixtures;

namespace Fip.Strive.Tracking.Web.UnitTests.Access;

/// <summary>
/// The cookie's attributes, which the sign-in tests deliberately do not cover — they assert a
/// cookie is issued, and a development host issues a different one from the container. Everything
/// here runs as <c>Production</c>, because that is the configuration that actually ships.
/// </summary>
public class SessionCookieTests : TrackingWebTest
{
    private const string CookieName = "strive.tracking";

    [Fact]
    public async Task The_session_cookie_is_hardened_in_production()
    {
        var cookie = await SignInAndReadCookieAsync(ProductionClient());

        cookie.Should().Contain("httponly", "script must not be able to read the session");
        cookie.Should().Contain("samesite=lax", "a cross-site POST must not carry the session");

        // The flag that makes A6 real: a browser discards a Secure cookie delivered over plain
        // HTTP, so this attribute is also what makes TLS in front of the container mandatory.
        cookie.Should().Contain("secure");
    }

    [Fact]
    public async Task The_session_cookie_is_not_secure_in_development()
    {
        var cookie = await SignInAndReadCookieAsync(Client());

        // Development is served over HTTP, and a Secure cookie there could never come back.
        cookie.Should().NotContain("secure");
    }

    private static async Task<string> SignInAndReadCookieAsync(HttpClient client)
    {
        using var form = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("password", TrackingAppFactory.Password),
        ]);

        var response = await client.PostAsync("/auth/login", form);

        response.Headers.Contains("Set-Cookie").Should().BeTrue("signing in has to issue a cookie");

        var cookie = response
            .Headers.GetValues("Set-Cookie")
            .Single(value => value.StartsWith($"{CookieName}=", StringComparison.Ordinal));

        return cookie.ToLowerInvariant();
    }
}
