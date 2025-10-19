using Fip.Strive.Harvester.Web.Components;
using MudBlazor;
using MudBlazor.Services;

namespace Fip.Strive.Harvester.Web.Setup.Infrastructure;

public static class FrontendRegistrations
{
    public static void AddFrontend(this IServiceCollection services)
    {
        services.AddRazorComponents().AddInteractiveServerComponents();
        services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
        });
    }

    public static void UseFrontend(this WebApplication app)
    {
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
    }
}
