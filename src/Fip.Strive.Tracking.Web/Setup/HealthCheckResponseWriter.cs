using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Fip.Strive.Tracking.Web.Setup;

public static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };

    public static Task WriteJsonHealthResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(SerializeReport(report));
    }

    private static string SerializeReport(HealthReport report)
    {
        var payload = new
        {
            status = report.Status.ToString(),
            results = report.Entries.Select(ToEntrySummary),
        };

        return JsonSerializer.Serialize(payload, SerializerOptions);
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
