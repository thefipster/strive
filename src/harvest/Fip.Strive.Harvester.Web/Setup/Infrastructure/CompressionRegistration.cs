using Microsoft.AspNetCore.ResponseCompression;

namespace Fip.Strive.Harvester.Web.Setup.Infrastructure;

public static class CompressionRegistration
{
    public static void AddCompression(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
        });
    }

    public static void UseCompression(this WebApplication app)
    {
        app.UseResponseCompression();
    }
}
