using MudBlazor;
using MudBlazor.Services;

namespace Fip.Strive.Tracking.Web.Setup;

public static class FrontendRegistration
{
    public static void AddFrontend(this IServiceCollection services)
    {
        services.AddRazorComponents().AddInteractiveServerComponents();

        services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
        });
    }

    public static void UseFrontend<TApp>(this WebApplication app)
    {
        app.MapRazorComponents<TApp>().AddInteractiveServerRenderMode();
    }
}
