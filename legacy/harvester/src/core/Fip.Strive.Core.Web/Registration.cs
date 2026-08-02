using Fip.Strive.Core.Web.Setup;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Core.Web;

public static class Registration
{
    public static void AddCoreServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddCompression();
        services.AddMonitoring(configuration);
        services.AddHealthEndpoint();
        services.AddFrontend();
    }

    public static void UseCoreServices<TApp>(this WebApplication app)
    {
        app.UseCompression();
        app.UseErrorHandling();
        app.UseHttps();
        app.MapStaticAssets();
        app.UseAntiforgery();
        app.UseFrontend<TApp>();
        app.UseHealthEnpoint();
    }
}
