using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Core.Web.Setup;

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
