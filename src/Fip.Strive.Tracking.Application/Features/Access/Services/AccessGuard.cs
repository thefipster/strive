using System.Security.Cryptography;
using System.Text;
using Fip.Strive.Tracking.Application.Features.Access.Services.Contracts;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Tracking.Application.Features.Access.Services;

public sealed class AccessGuard(IOptions<AccessOptions> options) : IAccessGuard
{
    private readonly AccessOptions _options = options.Value;

    public string UserName =>
        string.IsNullOrWhiteSpace(_options.UserName) ? "user" : _options.UserName.Trim();

    public bool ApiEnabled => !string.IsNullOrWhiteSpace(_options.ApiKey);

    public bool IsPassword(string? candidate) =>
        Pbkdf2Password.Verify(candidate, _options.PasswordHash);

    /// <summary>
    /// Compared in fixed time. The key is a shared secret rather than a hash, so an ordinary string
    /// comparison would leak its prefix to anyone patient enough to measure.
    /// </summary>
    public bool IsApiKey(string? candidate)
    {
        if (!ApiEnabled || string.IsNullOrEmpty(candidate))
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(candidate),
            Encoding.UTF8.GetBytes(_options.ApiKey)
        );
    }
}
