using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Fip.Strive.Web.Setup;

public static class HealthCheckRegistration
{
    /// <summary>
    /// Readiness is "can this instance serve the catalog", which means Postgres has to answer.
    /// Without a registered check the endpoint would report healthy for a process that is merely
    /// listening — and both the Aspire AppHost and any container probe would believe it.
    /// </summary>
    public static void AddHealthEndpoint(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck<CatalogHealthCheck>(
                CatalogHealthCheck.Name,
                tags: ["ready"],
                timeout: CatalogHealthCheck.Timeout
            );
    }

    /// <summary>
    /// Public, and deliberately so: it is what tells the orchestrator to restart this container,
    /// and it hands out nothing but a status per check.
    /// </summary>
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
