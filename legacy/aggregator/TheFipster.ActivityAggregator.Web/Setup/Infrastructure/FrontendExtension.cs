using MudBlazor;
using MudBlazor.Services;

namespace TheFipster.ActivityAggregator.Web.Setup.Infrastructure;

public static class FrontendExtension
{
    public static void AddFrontend(this IServiceCollection services)
    {
        services.AddRazorComponents().AddInteractiveServerComponents();
        services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
        });
    }
}
