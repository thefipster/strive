namespace Fip.Strive.Web.Setup;

public static class PipelineRegistration
{
    public static void UseErrorHandling(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            app.UseExceptionHandler("/error", createScopeForErrors: true);
    }

    public static void UseHttps(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            app.UseHsts();

        app.UseHttpsRedirection();
    }
}
