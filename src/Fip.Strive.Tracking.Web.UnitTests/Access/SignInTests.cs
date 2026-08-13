using System.Net;
using AwesomeAssertions;
using Fip.Strive.Tracking.Web.UnitTests.Fixtures;

namespace Fip.Strive.Tracking.Web.UnitTests.Access;

/// <summary>
/// One user, one password, and authorization applied to the endpoints rather than to each page —
/// so a page added later is protected by default. That property is what the first test pins.
/// </summary>
public class SignInTests : TrackingWebTest
{
    private const string CookieName = "strive.tracking";

    [Theory]
    [InlineData("/")]
    [InlineData("/trackers/0f8fad5b-d9cb-469f-a165-70867728950e")]
    [InlineData("/trackers/0f8fad5b-d9cb-469f-a165-70867728950e/fields")]
    public async Task Pages_send_a_signed_out_visitor_to_the_login(string path)
    {
        var response = await Client().GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.Found);
        RedirectPath(response).Should().Be("/login.html");
    }

    [Fact]
    public async Task The_right_password_sets_the_session_cookie()
    {
        var response = await PostPasswordAsync(TrackingAppFactory.Password);

        response.StatusCode.Should().Be(HttpStatusCode.Found);
        response.Headers.Location?.OriginalString.Should().Be("/");

        response.Headers.Contains("Set-Cookie").Should().BeTrue("signing in has to issue a cookie");

        response
            .Headers.GetValues("Set-Cookie")
            .Should()
            .Contain(cookie => cookie.StartsWith($"{CookieName}=", StringComparison.Ordinal));
    }

    [Fact]
    public async Task The_wrong_password_comes_back_with_an_error_and_no_cookie()
    {
        var response = await PostPasswordAsync("not the password");

        response.StatusCode.Should().Be(HttpStatusCode.Found);
        response.Headers.Location?.OriginalString.Should().Be("/login.html?error=1");
        response.Headers.Contains("Set-Cookie").Should().BeFalse();
    }

    /// <summary>
    /// Where a redirect points, without its host or its query. The cookie handler challenges with
    /// an absolute URI carrying a <c>ReturnUrl</c>, while the endpoints below redirect with a
    /// relative one — so only the path is a fair comparison across both.
    /// </summary>
    private static string RedirectPath(HttpResponseMessage response)
    {
        var location = response.Headers.Location;
        location.Should().NotBeNull("a redirect has to say where to");

        return location!.IsAbsoluteUri ? location.AbsolutePath : location.OriginalString;
    }

    private async Task<HttpResponseMessage> PostPasswordAsync(string password) =>
        await Client()
            .PostAsync(
                "/auth/login",
                new FormUrlEncodedContent([new KeyValuePair<string, string>("password", password)])
            );
}
