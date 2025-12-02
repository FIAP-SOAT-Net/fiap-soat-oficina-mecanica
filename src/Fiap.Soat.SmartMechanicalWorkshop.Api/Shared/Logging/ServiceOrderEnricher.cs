using Serilog.Core;
using Serilog.Events;

namespace Fiap.Soat.SmartMechanicalWorkshop.Api.Shared.Logging;

public sealed class ServiceOrderEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ServiceOrderEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return;
        }

        if (httpContext.TraceIdentifier is not null)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestId", httpContext.TraceIdentifier));
        }

        if (httpContext.Items.TryGetValue("OrderId", out var orderId))
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("OrderId", orderId));
        }

        if (httpContext.Items.TryGetValue("CustomerId", out var customerId))
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CustomerId", customerId));
        }
    }
}
