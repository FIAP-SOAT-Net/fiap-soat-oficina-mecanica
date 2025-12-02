using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;
using System.Text.Json;

namespace Fiap.Soat.SmartMechanicalWorkshop.Api.Shared.HealthChecks;

public static class HealthCheckResponseWriter
{
    public static Task WriteResponse(HttpContext context, HealthReport healthReport)
    {
        context.Response.ContentType = "application/json";

        var result = JsonSerializer.Serialize(new
        {
            status = healthReport.Status.ToString(),
            version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown",
            timestamp = DateTimeOffset.UtcNow.ToString("o"),
            duration = healthReport.TotalDuration.TotalMilliseconds,
            checks = healthReport.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration.TotalMilliseconds,
                exception = entry.Value.Exception?.Message,
                data = entry.Value.Data
            })
        }, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return context.Response.WriteAsync(result);
    }
}
