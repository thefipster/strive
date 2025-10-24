using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Fip.Strive.Harvester.Web.Setup.Infrastructure;

public static class HealthCheckRegistration
{
    public static void UseHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = WriteJsonHealthResponse
        });

        app.MapHealthChecks("/health/queue", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("queue"),
            ResponseWriter = WriteJsonHealthResponse
        });
    }

    private static Task WriteJsonHealthResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var json = SerializeReport(report);
        return context.Response.WriteAsync(json);
    }

    private static string SerializeReport(HealthReport report)
    {
        var payload = new
        {
            status = report.Status.ToString(),
            results = report.Entries.Select(ToEntrySummary),
        };

        return JsonSerializer.Serialize(
            payload,
            new JsonSerializerOptions
            {
                WriteIndented = true, // optional
            }
        );
    }

    private static object ToEntrySummary(KeyValuePair<string, HealthReportEntry> entry)
    {
        var e = entry.Value;
        return new
        {
            name = entry.Key,
            status = e.Status.ToString(),
            description = e.Description,
            durationMs = e.Duration.TotalMilliseconds,
            data = e.Data.Count > 0 ? e.Data : null,
        };
    }
}
