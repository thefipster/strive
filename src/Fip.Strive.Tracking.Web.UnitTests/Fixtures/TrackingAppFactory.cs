using Fip.Strive.Tracking.Application.Features.Access;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Fip.Strive.Tracking.Web.UnitTests.Fixtures;

/// <summary>
/// Boots the real host against a throwaway data directory. Everything the app refuses to start
/// without is supplied here, and nothing else is stubbed — the point of these tests is that the
/// pipeline as assembled in <c>Program.cs</c> guards what it claims to guard.
/// </summary>
/// <param name="environment">
/// Null leaves the default. Naming <c>Production</c> is what reaches the half of the pipeline that
/// is written as <c>if (!IsDevelopment())</c> — the Secure cookie, HSTS and the HTTPS redirect —
/// none of which a development host ever runs.
/// </param>
public sealed class TrackingAppFactory(
    string dataDirectory,
    string? apiKey,
    string? environment = null,
    IReadOnlyDictionary<string, string>? settings = null
) : WebApplicationFactory<Program>
{
    public const string Password = "correct horse battery staple";

    /// <summary>32 characters, which is the floor <c>AccessOptions</c> refuses to start below.</summary>
    public const string ApiKey = "0123456789abcdef0123456789abcdef";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (environment is not null)
            builder.UseEnvironment(environment);

        builder.UseSetting("Tracking:DataDirectory", dataDirectory);

        // Hashed here rather than pasted in as a literal, so the cost factor can move without a
        // test knowing about it — and so the login tests exercise the real verify path.
        builder.UseSetting("Access:PasswordHash", Pbkdf2Password.Create(Password));

        // Empty means no API key at all, which turns the pull API off and leaves /health open.
        builder.UseSetting("Access:ApiKey", apiKey ?? string.Empty);

        foreach (var (key, value) in settings ?? new Dictionary<string, string>())
            builder.UseSetting(key, value);
    }
}
