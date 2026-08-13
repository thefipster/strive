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

    /// <summary>
    /// Every page and the circuit behind them require a signed-in user. Enforced on the endpoints
    /// rather than per page, so a page added later is protected by default instead of by memory.
    /// </summary>
    public static void UseFrontend<TApp>(this WebApplication app)
    {
        app.MapRazorComponents<TApp>().AddInteractiveServerRenderMode().RequireAuthorization();
    }
}
