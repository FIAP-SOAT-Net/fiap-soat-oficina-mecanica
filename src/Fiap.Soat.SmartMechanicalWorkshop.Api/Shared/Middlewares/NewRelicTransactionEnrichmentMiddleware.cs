using Fiap.Soat.SmartMechanicalWorkshop.Api.Shared.Services;
using System.Diagnostics.CodeAnalysis;

namespace Fiap.Soat.SmartMechanicalWorkshop.Api.Shared.Middlewares;

[ExcludeFromCodeCoverage]
public sealed class NewRelicTransactionEnrichmentMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<NewRelicTransactionEnrichmentMiddleware> _logger;

    public NewRelicTransactionEnrichmentMiddleware(RequestDelegate next, ILogger<NewRelicTransactionEnrichmentMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, INewRelicTelemetryService newRelicService)
    {
        try
        {
            // Add standard attributes to transaction
            newRelicService.AddCustomAttribute("request.method", context.Request.Method);
            newRelicService.AddCustomAttribute("request.path", context.Request.Path.Value ?? "/");

            if (context.Request.Headers.ContainsKey("User-Agent"))
            {
                newRelicService.AddCustomAttribute("request.userAgent", context.Request.Headers["User-Agent"].ToString());
            }

            // Extract customer/order IDs from route if present
            if (context.Request.RouteValues.TryGetValue("id", out var idValue) && idValue != null)
            {
                newRelicService.AddCustomAttribute("entity.id", idValue.ToString() ?? "unknown");
            }

            await _next(context);

            // Add response status
            newRelicService.AddCustomAttribute("response.statusCode", context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error in New Relic transaction enrichment middleware");
            // Continue processing even if enrichment fails
            await _next(context);
        }
    }
}
