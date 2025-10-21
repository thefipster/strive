using Fip.Strive.Harvester.Application;
using Fip.Strive.Harvester.Application.Core.Hubs;

namespace Fip.Strive.Harvester.Web.Setup;

public static class ApplicationServices
{
    public static void AddApplication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddApplicationServices(configuration);
    }

    public static void UseApplication(this WebApplication app)
    {
        app.MapHub<HelloWorldHub>($"/hubs/{HelloWorldHub.HubName}");
    }
}
