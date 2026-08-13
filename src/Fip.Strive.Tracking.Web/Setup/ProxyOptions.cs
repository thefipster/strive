namespace Fip.Strive.Tracking.Web.Setup;

/// <summary>
/// How the app is reached. The container speaks plain HTTP and expects TLS to be terminated in
/// front of it, which means the scheme and the caller's address only exist in headers the proxy
/// adds. Without this the app believes every request arrived over HTTP from the proxy itself.
/// </summary>
public sealed class ProxyOptions
{
    public const string SectionName = "Proxy";

    /// <summary>
    /// Off by default, because trusting <c>X-Forwarded-For</c> from an untrusted caller is worse
    /// than ignoring it: anyone able to reach the app directly could then vary the header per
    /// request and step around the login rate limit, which partitions on the address.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Addresses of the proxies to trust. Leave empty when the proxy's address is not stable — a
    /// container on a compose network rarely has one — and the immediate upstream is trusted
    /// instead. That is only safe while nothing but the proxy can reach the app.
    /// </summary>
    public string[] KnownProxies { get; set; } = [];

    /// <summary>Networks to trust, in CIDR form, for the same purpose as <see cref="KnownProxies"/>.</summary>
    public string[] KnownNetworks { get; set; } = [];
}
