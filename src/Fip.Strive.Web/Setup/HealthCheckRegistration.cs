using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Fip.Strive.Web.Setup;

public static class HealthCheckRegistration
{
    public static void AddHealthEndpoint(this IServiceCollection services)
    {
        services.AddHealthChecks();
    }

    public static void UseHealthEndpoint(this WebApplication app)
    {
        app.MapHealthChecks(
            "/health",
            new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = HealthCheckResponseWriter.WriteJsonHealthResponse,
            }
        );
    }
}
