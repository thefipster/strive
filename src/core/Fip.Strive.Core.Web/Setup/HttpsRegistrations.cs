using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Fip.Strive.Core.Web.Setup;

public static class HttpsRegistrations
{
    public static void UseHttps(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            app.UseHsts();

        app.UseHttpsRedirection();
    }
}
