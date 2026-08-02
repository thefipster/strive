using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Fip.Strive.Core.Web.Setup;

public static class MonitoringRegistrations
{
    public static void AddMonitoring(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog(c => c.ReadFrom.Configuration(configuration));
    }
}
