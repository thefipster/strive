using System.Threading.RateLimiting;
using Fip.Strive.Tracking.Application.Features.Access;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;

namespace Fip.Strive.Tracking.Web.Setup;

public static class AccessRegistration
{
    /// <summary>Where an unauthenticated request is sent. A plain file — it needs no circuit.</summary>
    public const string LoginPath = "/login.html";

    /// <summary>Rate limit applied to the login form, and only to it.</summary>
    public const string LoginRateLimit = "login";

    public static void AddAccess(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment
    )
    {
        var options =
            configuration.GetSection(AccessOptions.SectionName).Get<AccessOptions>()
            ?? new AccessOptions();

        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(cookie =>
            {
                cookie.LoginPath = LoginPath;
                cookie.AccessDeniedPath = LoginPath;
                cookie.LogoutPath = "/auth/logout";

                cookie.Cookie.Name = "strive.tracking";
                cookie.Cookie.HttpOnly = true;

                // Lax rather than Strict so following a link into the app keeps you signed in, and
                // it is what makes the antiforgery-free sign-out form below safe: a cross-site POST
                // does not carry the cookie.
                cookie.Cookie.SameSite = SameSiteMode.Lax;

                // Anywhere but a dev box this is served over TLS, and a session cookie that can
                // travel in clear text is not worth having.
                cookie.Cookie.SecurePolicy = environment.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;

                cookie.ExpireTimeSpan = TimeSpan.FromDays(Math.Clamp(options.SessionDays, 1, 365));
                cookie.SlidingExpiration = true;
            });

        services.AddAuthorization();
        services.AddCascadingAuthenticationState();

        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Per caller address. Behind a reverse proxy every request shares the proxy's address
            // and therefore one bucket — which still bounds a brute force, just less precisely.
            limiter.AddPolicy(
                LoginRateLimit,
                http =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(5),
                            QueueLimit = 0,
                        }
                    )
            );
        });
    }

    public static void UseAccess(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
    }
}
