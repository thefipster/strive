namespace Fip.Strive.Tracking.Application.Features.Access;

/// <summary>
/// Who gets in. There is exactly one user — this app is a personal input cache, not a service with
/// accounts — so there is no user table, just a hash in configuration.
/// </summary>
public sealed class AccessOptions
{
    public const string SectionName = "Access";

    /// <summary>Shown in the app bar and put on the sign-in cookie. Cosmetic; the password is the credential.</summary>
    public string UserName { get; set; } = "admin";

    /// <summary>
    /// PBKDF2 hash as produced by <c>Pbkdf2Password.Create</c>. Generate one with
    /// <c>dotnet run --project src/Fip.Strive.Tracking.Web -- hash-password</c> and supply it as
    /// <c>Access__PasswordHash</c>. Required — the app refuses to start without it.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Shared secret for the read-only API the homelab pulls with. Leave empty and the API is not
    /// mapped at all.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>How long a sign-in survives. Sliding, so daily use never has to log in again.</summary>
    public int SessionDays { get; set; } = 14;

    /// <summary>
    /// Short keys are the whole attack surface of the API, and this app is meant to sit on the open
    /// internet. 32 characters of a generated secret is the floor, not a recommendation.
    /// </summary>
    public const int MinimumApiKeyLength = 32;
}
