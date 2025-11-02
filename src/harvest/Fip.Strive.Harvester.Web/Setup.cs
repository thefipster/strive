using Fip.Strive.Core.Web.Extensions;
using Fip.Strive.Harvester.Application.Core.Hubs;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Fip.Strive.Harvester.Web;

public static class ApplicationServices
{
    public static void UseApplicationServices(this WebApplication app)
    {
        app.MapHub<HelloWorldHub>($"/hubs/{HelloWorldHub.HubName}");
        app.MapHub<QueueHub>($"/hubs/{QueueHub.HubName}");

        app.MapHealthChecks(
            "/health/queue",
            new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("queue"),
                ResponseWriter = HealthCheckResponseWriter.WriteJsonHealthResponse,
            }
        );
    }
}
