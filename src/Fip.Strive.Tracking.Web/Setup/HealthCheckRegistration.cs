using Fip.Strive.Tracking.Application.Features.Access.Services.Contracts;
using Fip.Strive.Tracking.Web.Api;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Fip.Strive.Tracking.Web.Setup;

public static class HealthCheckRegistration
{
    /// <summary>
    /// Readiness is "can this instance read its database", which means querying the SQLite file.
    /// Without a registered check the endpoint would report healthy for a process that is merely
    /// listening.
    /// </summary>
    public static void AddHealthEndpoint(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck<TrackingHealthCheck>(
                TrackingHealthCheck.Name,
                tags: ["ready"],
                timeout: TrackingHealthCheck.Timeout
            );
    }

    /// <summary>
    /// Mapped as a route handler rather than with <c>MapHealthChecks</c>, because only a route
    /// handler runs endpoint filters — <see cref="ApiKeyFilter"/> attached to a
    /// <c>RequestDelegate</c> endpoint compiles and is then never invoked, which would leave the
    /// endpoint open while looking guarded.
    /// </summary>
    /// <remarks>
    /// The handler takes <see cref="HealthCheckService"/> as a second parameter, and has to keep
    /// taking something beyond the context. A lambda whose only parameter is an
    /// <see cref="HttpContext"/> matches <c>RequestDelegate</c> exactly, which routes around the
    /// factory that builds the filter pipeline — the key check would silently stop running.
    /// </remarks>
    public static void UseHealthEndpoint(this WebApplication app)
    {
        var guard = app.Services.GetRequiredService<IAccessGuard>();

        var health = app.MapGet(
            "/health",
            async (HttpContext http, HealthCheckService checks) =>
            {
                var report = await checks.CheckHealthAsync(http.RequestAborted);

                // What MapHealthChecks would have returned: degraded still counts as serving.
                http.Response.StatusCode =
                    report.Status == HealthStatus.Unhealthy
                        ? StatusCodes.Status503ServiceUnavailable
                        : StatusCodes.Status200OK;

                await HealthCheckResponseWriter.WriteJsonHealthResponse(http, report);
            }
        );

        // With no key configured there is nothing to authenticate with, and a permanently 401
        // readiness probe reads as a permanently unhealthy container. Same rule the pull API
        // follows: no key means the guard is not there, not that everything is refused.
        if (!guard.ApiEnabled)
        {
            app.Logger.LogInformation(
                "No {Section} key configured — /health is public",
                "Access:ApiKey"
            );

            return;
        }

        health.AddEndpointFilter<ApiKeyFilter>();

        app.Logger.LogInformation("/health requires the {Header} header", ApiKeyFilter.HeaderName);
    }
}
