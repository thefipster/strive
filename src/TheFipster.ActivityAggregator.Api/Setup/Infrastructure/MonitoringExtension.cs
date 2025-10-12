using System.Reflection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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
        services.AddMetrics(environment);
    }

    public static void AddMetrics(this IServiceCollection services, IHostEnvironment environment)
    {
        var resourceBuilder = ResourceBuilder
            .CreateDefault()
            .AddService(
                nameof(Api),
                serviceVersion: Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
            );

        services.AddMetrics();
        services
            .AddOpenTelemetry()
            .ConfigureResource(c => c.AddService(nameof(Api)))
            .WithMetrics(provider =>
            {
                provider
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
                provider.AddMeter(
                    "Microsoft.AspNetCore.Hosting",
                    "Microsoft.AspNetCore.Server.Kestrel",
                    "System.Net.Http"
                );
            })
            .WithTracing(options =>
            {
                if (environment.IsDevelopment())
                {
                    options.SetSampler<AlwaysOnSampler>();
                }
                options.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation();
                options.AddSource(nameof(Api));
            });
    }
}
