namespace Fip.Strive.Harvester.Web.Setup.Infrastructure;

public static class ErrorHandlerRegistration
{
    public static void UseErrorHandling(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
    }
}
