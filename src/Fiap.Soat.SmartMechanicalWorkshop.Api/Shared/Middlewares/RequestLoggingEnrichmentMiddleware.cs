using Serilog.Context;

namespace Fiap.Soat.SmartMechanicalWorkshop.Api.Shared.Middlewares;

public sealed class RequestLoggingEnrichmentMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingEnrichmentMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var traceParent = context.Request.Headers["traceparent"].FirstOrDefault();
        var newRelicHeader = context.Request.Headers["newrelic"].FirstOrDefault();
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? context.TraceIdentifier;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestPath", context.Request.Path))
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        using (LogContext.PushProperty("UserAgent", context.Request.Headers["User-Agent"].FirstOrDefault()))
        using (LogContext.PushProperty("RemoteIpAddress", context.Connection.RemoteIpAddress?.ToString()))
        {
            if (!string.IsNullOrEmpty(traceParent))
            {
                LogContext.PushProperty("TraceParent", traceParent);
            }

            if (!string.IsNullOrEmpty(newRelicHeader))
            {
                LogContext.PushProperty("NewRelicTrace", newRelicHeader);
            }

            await _next(context);
        }
    }
}
