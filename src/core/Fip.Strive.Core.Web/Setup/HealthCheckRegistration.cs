using System.Text.Json;
using Fip.Strive.Core.Web.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Core.Web.Setup;

public static class HealthCheckRegistration
{
    public static void AddHealthEndpoint(this IServiceCollection services)
    {
        services.AddHealthChecks();
    }

    public static void UseHealthEnpoint(this WebApplication app)
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
