using AwesomeAssertions;
using Fip.Strive.Tracking.Application.Features.Access;
using Fip.Strive.Tracking.Application.Features.Access.Services;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Tracking.Application.UnitTests.Access;

public class AccessGuardTests
{
    private const string Key = "0123456789abcdef0123456789abcdef";

    [Fact]
    public void The_configured_password_is_the_only_one_that_gets_in()
    {
        var guard = Guard(password: "hunter2");

        guard.IsPassword("hunter2").Should().BeTrue();
        guard.IsPassword("hunter3").Should().BeFalse();
        guard.IsPassword(null).Should().BeFalse();
    }

    [Fact]
    public void An_api_key_has_to_match_exactly()
    {
        var guard = Guard(apiKey: Key);

        guard.IsApiKey(Key).Should().BeTrue();
        guard.IsApiKey(Key.ToUpperInvariant()).Should().BeFalse();
        guard.IsApiKey(Key[..^1]).Should().BeFalse();
        guard.IsApiKey($"{Key} ").Should().BeFalse();
    }

    [Fact]
    public void Without_a_configured_key_the_api_is_off_and_nothing_matches()
    {
        var guard = Guard();

        guard.ApiEnabled.Should().BeFalse();
        guard.IsApiKey(null).Should().BeFalse();
        guard.IsApiKey(string.Empty).Should().BeFalse();

        // The interesting one: an empty configured key must not turn into "any empty header is fine".
        guard.IsApiKey("   ").Should().BeFalse();
    }

    [Fact]
    public void A_blank_user_name_falls_back_rather_than_showing_an_empty_app_bar()
    {
        Guard(userName: "  felix  ").UserName.Should().Be("felix");
        Guard(userName: "   ").UserName.Should().Be("user");
    }

    private static AccessGuard Guard(
        string password = "hunter2",
        string apiKey = "",
        string userName = "admin"
    ) =>
        new(
            Options.Create(
                new AccessOptions
                {
                    UserName = userName,
                    PasswordHash = Pbkdf2Password.Create(password),
                    ApiKey = apiKey,
                }
            )
        );
}
