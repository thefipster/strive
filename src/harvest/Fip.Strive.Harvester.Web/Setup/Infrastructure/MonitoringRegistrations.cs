using Serilog;

namespace Fip.Strive.Harvester.Web.Setup.Infrastructure;

public static class MonitoringRegistrations
{
    public static void AddMonitoring(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog(c => c.ReadFrom.Configuration(configuration));
    }
}
