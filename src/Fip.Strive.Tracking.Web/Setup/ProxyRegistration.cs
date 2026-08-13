using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace Fip.Strive.Tracking.Web.Setup;

public static class ProxyRegistration
{
    public static void AddProxy(this IServiceCollection services, IConfiguration configuration)
    {
        var options =
            configuration.GetSection(ProxyOptions.SectionName).Get<ProxyOptions>()
            ?? new ProxyOptions();

        services.Configure<ProxyOptions>(configuration.GetSection(ProxyOptions.SectionName));

        if (!options.Enabled)
            return;

        // Parsed here rather than inside the callback below, so a typo is refused while the host is
        // being built — the same treatment a malformed password hash gets. Silently dropping an
        // unparseable entry would be the worst outcome available: this list decides who is trusted
        // to rewrite a caller's address, and a narrower list than the operator wrote is a change to
        // the security posture that nothing would report.
        var proxies = options.KnownProxies.Select(ParseAddress).ToArray();
        var networks = options.KnownNetworks.Select(ParseNetwork).ToArray();

        services.Configure<ForwardedHeadersOptions>(forwarded =>
        {
            // Only these two. X-Forwarded-Host is not honoured: nothing here builds absolute URLs
            // from the host, and accepting it invites cache and redirect poisoning for no gain.
            forwarded.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            // The defaults trust only loopback, which is never what a container sees — the proxy is
            // another host on the bridge network. Cleared first, then narrowed if configuration
            // names anything.
            forwarded.KnownProxies.Clear();
            forwarded.KnownIPNetworks.Clear();

            foreach (var address in proxies)
                forwarded.KnownProxies.Add(address);

            foreach (var network in networks)
                forwarded.KnownIPNetworks.Add(network);
        });
    }

    private static IPAddress ParseAddress(string value) =>
        IPAddress.TryParse(value, out var address)
            ? address
            : throw new InvalidOperationException(
                $"'{ProxyOptions.SectionName}:{nameof(ProxyOptions.KnownProxies)}' contains "
                    + $"'{value}', which is not an IP address."
            );

    // Fully qualified: Microsoft.AspNetCore.HttpOverrides has its own IPNetwork, and it is the
    // deprecated one.
    private static System.Net.IPNetwork ParseNetwork(string value) =>
        System.Net.IPNetwork.TryParse(value, out var network)
            ? network
            : throw new InvalidOperationException(
                $"'{ProxyOptions.SectionName}:{nameof(ProxyOptions.KnownNetworks)}' contains "
                    + $"'{value}', which is not a CIDR network such as 10.0.0.0/8."
            );

    /// <summary>
    /// Has to run before anything that reads the scheme or the caller's address — request logging,
    /// HSTS, the HTTPS redirect and the login rate limiter all do.
    /// </summary>
    public static void UseProxy(this WebApplication app)
    {
        var options =
            app.Configuration.GetSection(ProxyOptions.SectionName).Get<ProxyOptions>()
            ?? new ProxyOptions();

        if (!options.Enabled)
        {
            WarnIfSignInCannotWork(app);
            return;
        }

        app.UseForwardedHeaders();

        if (options.KnownProxies.Length == 0 && options.KnownNetworks.Length == 0)
            app.Logger.LogWarning(
                "{Section}:Enabled is on with no {Proxies} or {Networks}, so the immediate upstream "
                    + "is trusted for the caller's address and scheme. Safe only while nothing but "
                    + "the proxy can reach this app.",
                ProxyOptions.SectionName,
                nameof(ProxyOptions.KnownProxies),
                nameof(ProxyOptions.KnownNetworks)
            );
        else
            app.Logger.LogInformation(
                "Trusting forwarded headers from {ProxyCount} proxies and {NetworkCount} networks",
                options.KnownProxies.Length,
                options.KnownNetworks.Length
            );
    }

    /// <summary>
    /// The combination that produces a login nobody can complete: the cookie is issued with
    /// <c>Secure</c> outside development, a browser discards a <c>Secure</c> cookie that arrived
    /// over plain HTTP, and the sign-in then redirects to a page that sends it back to the login.
    /// Nothing fails, so there is no error to find — hence a warning at startup instead.
    /// </summary>
    private static void WarnIfSignInCannotWork(WebApplication app)
    {
        if (app.Environment.IsDevelopment() || HasHttpsEndpoint(app.Configuration))
            return;

        app.Logger.LogWarning(
            "No HTTPS endpoint is configured and {Section}:Enabled is off. The session cookie is "
                + "issued with Secure, so a browser reaching this over plain HTTP discards it and "
                + "sign-in loops back to the login page. Terminate TLS in front of this and set "
                + "{Section}__Enabled=true.",
            ProxyOptions.SectionName,
            ProxyOptions.SectionName
        );
    }

    private static bool HasHttpsEndpoint(IConfiguration configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration["ASPNETCORE_HTTPS_PORTS"]))
            return true;

        if (
            configuration["ASPNETCORE_URLS"]
                ?.Contains("https://", StringComparison.OrdinalIgnoreCase) == true
        )
            return true;

        // Kestrel endpoints configured in appsettings rather than by environment variable.
        return configuration
            .GetSection("Kestrel:Endpoints")
            .GetChildren()
            .Any(endpoint =>
                endpoint["Url"]?.Contains("https://", StringComparison.OrdinalIgnoreCase) == true
            );
    }
}
