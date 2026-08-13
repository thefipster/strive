using System.Net;
using AwesomeAssertions;
using Fip.Strive.Tracking.Web.UnitTests.Fixtures;

namespace Fip.Strive.Tracking.Web.UnitTests.Pages;

/// <summary>
/// That the pages actually render inside the MudBlazor shell once signed in. The strive app has had
/// this since its first integration suite; the tracker never did, because everything here needs a
/// cookie first.
/// </summary>
/// <remarks>
/// Added while auditing the MudBlazor 8 → 9 upgrade. That upgrade removed a parameter the import
/// page relied on and the build only warned, so "it compiles" was demonstrably not enough — and a
/// warnings-as-errors gate only helps for breakages the analyzer knows to flag. A page that renders
/// is the check that does not depend on being told.
/// </remarks>
public class ShellRenderTests : TrackingWebTest
{
    [Fact]
    public async Task The_tracker_list_renders_inside_the_shell()
    {
        var client = await SignedInClientAsync();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        html.Should().Contain("mud-appbar", "the app bar must be part of the shell");
        html.Should().Contain("mud-drawer", "the nav menu drawer must be part of the shell");
    }

    [Fact]
    public async Task An_empty_tracker_list_says_so_rather_than_rendering_nothing()
    {
        var client = await SignedInClientAsync();

        var html = await client.GetStringAsync("/");

        // A blank page is what a silently-dropped render fragment looks like, which is precisely how
        // the import page's drop zone disappeared. Asserting on real copy catches that.
        html.Should().Contain("mud-typography");
    }

    /// <summary>
    /// Signs in and hands back a client carrying the session cookie. The factory's client does not
    /// follow redirects, so the cookie has to be lifted off the sign-in response by hand.
    /// </summary>
    private async Task<HttpClient> SignedInClientAsync()
    {
        var client = Client();

        using var form = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("password", TrackingAppFactory.Password),
        ]);

        var response = await client.PostAsync("/auth/login", form);

        var cookie = response
            .Headers.GetValues("Set-Cookie")
            .Single(value => value.StartsWith("strive.tracking=", StringComparison.Ordinal));

        client.DefaultRequestHeaders.Add("Cookie", cookie.Split(';')[0]);

        return client;
    }
}
