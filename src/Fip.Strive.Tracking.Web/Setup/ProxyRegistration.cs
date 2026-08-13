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

            foreach (var proxy in options.KnownProxies)
                if (IPAddress.TryParse(proxy, out var address))
                    forwarded.KnownProxies.Add(address);

            // Fully qualified: Microsoft.AspNetCore.HttpOverrides has its own IPNetwork, and it is
            // the deprecated one.
            foreach (var network in options.KnownNetworks)
                if (System.Net.IPNetwork.TryParse(network, out var parsed))
                    forwarded.KnownIPNetworks.Add(parsed);
        });
    }

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
