using Serilog;

namespace TheFipster.ActivityAggregator.Api.Setup;

public static class LoggingExtension
{
    public static void AddLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog(c => c.ReadFrom.Configuration(configuration));
    }
}
