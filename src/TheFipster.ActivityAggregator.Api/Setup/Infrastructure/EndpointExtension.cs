using TheFipster.ActivityAggregator.Api.Hubs;
using TheFipster.ActivityAggregator.Domain;

namespace TheFipster.ActivityAggregator.Api.Setup.Infrastructure;

public static class EndpointExtension
{
    public static void AddEndpoints(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApiEndpoint();
        services.AddSignalR(e =>
        {
            e.EnableDetailedErrors = true;
            e.MaximumReceiveMessageSize = 102400000;
        });
    }

    public static void UseEndpoints(this WebApplication app)
    {
        app.MapControllers();
        app.MapHubs();
    }

    private static void MapHubs(this WebApplication app)
    {
        app.MapHub<ImportHub>(Defaults.Hubs.Importer.Url);
    }
}
