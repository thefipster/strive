using MudBlazor;
using MudBlazor.Services;

namespace Fip.Strive.Web.Setup;

public static class FrontendRegistration
{
    /// <summary>
    /// Blazor's default circuit message size (32 KB) is far too small for streaming multi-gigabyte
    /// takeout archives through <c>InputFile</c>, so the receive limit is raised to fit whole
    /// upload chunks. Nothing is buffered in memory beyond one chunk.
    /// </summary>
    private const long MaximumReceiveMessageSize = 10 * 1024 * 1024;

    public static void AddFrontend(this IServiceCollection services)
    {
        services
            .AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddHubOptions(options => options.MaximumReceiveMessageSize = MaximumReceiveMessageSize);

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
