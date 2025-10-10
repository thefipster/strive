using System.Reflection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace TheFipster.ActivityAggregator.Api.Setup;

public static class MetricsExtension
{
    public static void AddMetrics(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        var resourceBuilder = ResourceBuilder
            .CreateDefault()
            .AddService(
                "api",
                serviceVersion: Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
            );

        services
            .AddMetrics()
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
            });

        if (!string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            services
                .Configure<OpenTelemetryLoggerOptions>(options => options.AddOtlpExporter())
                .ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter())
                .ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddOtlpExporter());
        }
    }
}
