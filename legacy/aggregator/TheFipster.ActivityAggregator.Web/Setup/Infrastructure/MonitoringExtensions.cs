using Serilog;
using SerilogTracing;

namespace TheFipster.ActivityAggregator.Web.Setup.Infrastructure;

public static class MonitoringExtensions
{
    public static void AddMonitoring(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog(c => c.ReadFrom.Configuration(configuration));

        using var listener = new ActivityListenerConfiguration()
            .Instrument.WithDefaultInstrumentation(false)
            .Instrument.HttpClientRequests(opts => opts.MessageTemplate = nameof(Web))
            .TraceToSharedLogger();
    }
}
