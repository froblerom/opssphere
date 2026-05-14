using System.Net.Mime;
using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpsSphere.Api.Middleware;

namespace OpsSphere.Api.Health;

public static class HealthResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static Task WriteSimpleAsync(HttpContext context, HealthReport report)
    {
        var correlationId = context.Items[CorrelationIdMiddleware.ItemsKey] as string;

        context.Response.ContentType = MediaTypeNames.Application.Json;

        var response = new
        {
            status = report.Status.ToString(),
            timestampUtc = DateTime.UtcNow,
            correlationId
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    public static Task WriteDetailsAsync(HttpContext context, HealthReport report)
    {
        var correlationId = context.Items[CorrelationIdMiddleware.ItemsKey] as string;

        context.Response.ContentType = MediaTypeNames.Application.Json;

        // Safe: report check names and statuses only.
        // Do NOT include exception messages, data, or dependency details that could leak secrets.
        var response = new
        {
            status = report.Status.ToString(),
            timestampUtc = DateTime.UtcNow,
            correlationId,
            durationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                durationMs = e.Value.Duration.TotalMilliseconds,
                description = e.Value.Status == HealthStatus.Healthy ? e.Value.Description : null
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
