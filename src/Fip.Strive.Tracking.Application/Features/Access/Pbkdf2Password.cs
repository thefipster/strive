using System.Globalization;
using System.Security.Cryptography;

namespace Fip.Strive.Tracking.Application.Features.Access;

/// <summary>
/// Password hashing for the single user. PBKDF2-HMAC-SHA256, encoded as
/// <c>pbkdf2-sha256$iterations$salt$hash</c> so the iteration count travels with the hash and old
/// hashes keep verifying after the cost is raised.
/// </summary>
public static class Pbkdf2Password
{
    private const string Algorithm = "pbkdf2-sha256";

    /// <summary>OWASP's floor for PBKDF2-HMAC-SHA256. Raise it, do not lower it.</summary>
    private const int Iterations = 210_000;

    private const int SaltBytes = 16;
    private const int HashBytes = 32;

    public static string Create(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltBytes);
        var hash = Derive(password, salt, Iterations, HashBytes);

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{Algorithm}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}"
        );
    }

    /// <summary>
    /// False for a wrong password and false for anything malformed — a garbled configuration value
    /// must read as "this password is wrong", never as an exception on the login path.
    /// </summary>
    public static bool Verify(string? password, string? encoded)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(encoded))
            return false;

        var parts = encoded.Split('$');

        if (parts.Length != 4 || parts[0] != Algorithm)
            return false;

        if (
            !int.TryParse(parts[1], CultureInfo.InvariantCulture, out var iterations)
            || iterations <= 0
        )
            return false;

        byte[] salt;
        byte[] expected;

        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (salt.Length == 0 || expected.Length == 0)
            return false;

        var actual = Derive(password, salt, iterations, expected.Length);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    /// <summary>Whether a configuration value is even shaped like something Verify could accept.</summary>
    public static bool IsEncodedHash(string? encoded) =>
        !string.IsNullOrWhiteSpace(encoded)
        && encoded.Split('$') is [Algorithm, _, _, _];

    private static byte[] Derive(string password, byte[] salt, int iterations, int length) =>
        Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, length);
}
