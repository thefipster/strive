using Fip.Strive.Portal.Web.Models;

namespace Fip.Strive.Portal.Web;

public static class ApplicationServices
{
    public static void AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<List<AppConfig>>(configuration.GetSection(AppConfig.ConfigSectionName));
    }
}
