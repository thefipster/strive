namespace Fip.Strive.Tracking.Application.Features.Access.Services.Contracts;

/// <summary>The two credentials this app knows: the user's password and the API's key.</summary>
public interface IAccessGuard
{
    string UserName { get; }

    /// <summary>False when no API key is configured, which is also when the API is not mapped.</summary>
    bool ApiEnabled { get; }

    bool IsPassword(string? candidate);

    bool IsApiKey(string? candidate);
}
