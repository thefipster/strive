using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Fip.Strive.Core.Web.Setup;

public static class ErrorHandlerRegistration
{
    public static void UseErrorHandling(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            app.UseExceptionHandler("/error", createScopeForErrors: true);
    }
}
