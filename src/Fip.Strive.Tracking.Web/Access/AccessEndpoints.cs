using System.Security.Claims;
using Fip.Strive.Tracking.Application.Features.Access.Services.Contracts;
using Fip.Strive.Tracking.Web.Setup;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Fip.Strive.Tracking.Web.Access;

public static class AccessEndpoints
{
    /// <summary>
    /// Sign-in and sign-out as plain form posts. They cannot be Blazor pages: setting a cookie
    /// needs an HTTP response, and an interactive component only has a SignalR circuit.
    /// </summary>
    public static void MapAccess(this WebApplication app)
    {
        // Antiforgery is off because neither form can carry a token — one is a static file, the
        // other posts from an interactive page. The SameSite=Lax cookie is what stops a cross-site
        // sign-out, and a forged sign-in achieves nothing an attacker could not do by visiting the
        // login page themselves.
        var access = app.MapGroup("/auth").DisableAntiforgery();

        access
            .MapPost(
                "/login",
                async (
                    HttpContext http,
                    IAccessGuard guard,
                    ILoggerFactory loggers,
                    [FromForm] string? password,
                    [FromForm] string? returnUrl
                ) =>
                {
                    var destination = LocalPathOrHome(returnUrl);

                    if (!guard.IsPassword(password))
                    {
                        loggers
                            .CreateLogger(typeof(AccessEndpoints))
                            .LogWarning(
                                "Rejected a sign-in from {Address}",
                                http.Connection.RemoteIpAddress
                            );

                        // Carried through the failure too, so a mistyped password does not quietly
                        // cost the destination that survived the first redirect. Omitted when it is
                        // just the home page, which the login already defaults to.
                        var query =
                            destination == "/"
                                ? "?error=1"
                                : $"?error=1&ReturnUrl={Uri.EscapeDataString(destination)}";

                        return Results.Redirect($"{AccessRegistration.LoginPath}{query}");
                    }

                    var identity = new ClaimsIdentity(
                        [new Claim(ClaimTypes.Name, guard.UserName)],
                        CookieAuthenticationDefaults.AuthenticationScheme
                    );

                    await http.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identity),
                        new AuthenticationProperties { IsPersistent = true }
                    );

                    return Results.LocalRedirect(destination);
                }
            )
            .RequireRateLimiting(AccessRegistration.LoginRateLimit);

        MapSignOut(access);
    }

    /// <summary>
    /// Where to land after signing in. Anything that is not a path on this host becomes the home
    /// page, which is what stops a crafted <c>?ReturnUrl=</c> from turning the login form into an
    /// open redirect — a login page that will forward anywhere is a phishing primitive.
    /// </summary>
    /// <remarks>
    /// Validated here rather than left to <see cref="Results.LocalRedirect"/>, which throws on a
    /// non-local URL. On this path an exception would turn a hostile link into a 500 on the one
    /// endpoint that has to keep working.
    /// </remarks>
    private static string LocalPathOrHome(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return "/";

        // "//evil.test" and "/\evil.test" are both read as protocol-relative URLs by browsers, so
        // a leading slash on its own is not enough to call something local.
        if (
            returnUrl[0] != '/'
            || returnUrl.StartsWith("//", StringComparison.Ordinal)
            || returnUrl.StartsWith("/\\", StringComparison.Ordinal)
        )
            return "/";

        return returnUrl;
    }

    private static void MapSignOut(RouteGroupBuilder access)
    {
        access.MapPost(
            "/logout",
            async (HttpContext http) =>
            {
                await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.Redirect(AccessRegistration.LoginPath);
            }
        );
    }
}
