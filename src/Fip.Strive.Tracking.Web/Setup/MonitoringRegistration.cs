using Serilog;

namespace Fip.Strive.Tracking.Web.Setup;

public static class MonitoringRegistration
{
    public static void AddMonitoring(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog(c => c.ReadFrom.Configuration(configuration));
    }

    public static void UseRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
    }
}
