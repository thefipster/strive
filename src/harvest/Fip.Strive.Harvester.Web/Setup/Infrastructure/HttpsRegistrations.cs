using Serilog;

namespace Fip.Strive.Harvester.Web.Setup.Infrastructure;

public static class HttpsRegistrations
{
    public static void UseHttps(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            app.UseHsts();

        app.UseHttpsRedirection();
    }
}
