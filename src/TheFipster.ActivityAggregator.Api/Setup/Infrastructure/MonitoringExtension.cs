using Serilog;

namespace TheFipster.ActivityAggregator.Api.Setup.Infrastructure;

public static class MonitoringExtension
{
    public static void AddMonitoring(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        services.AddSerilog(c => c.ReadFrom.Configuration(configuration));
        services.AddMetrics(configuration, environment);
    }
}
