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
                    [FromForm] string? password
                ) =>
                {
                    if (!guard.IsPassword(password))
                    {
                        loggers
                            .CreateLogger(typeof(AccessEndpoints))
                            .LogWarning(
                                "Rejected a sign-in from {Address}",
                                http.Connection.RemoteIpAddress
                            );

                        return Results.Redirect($"{AccessRegistration.LoginPath}?error=1");
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

                    return Results.LocalRedirect("/");
                }
            )
            .RequireRateLimiting(AccessRegistration.LoginRateLimit);

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
