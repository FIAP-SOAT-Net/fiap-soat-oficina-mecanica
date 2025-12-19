using Fiap.Soat.SmartMechanicalWorkshop.Api.Shared.Services;
using Fiap.Soat.SmartMechanicalWorkshop.Application.Shared.Services;

namespace Fiap.Soat.SmartMechanicalWorkshop.Api.Shared.Adapters;

public sealed class NewRelicTelemetryAdapter : ITelemetryService
{
    private readonly INewRelicTelemetryService _newRelicService;
    private readonly ILogger<NewRelicTelemetryAdapter> _logger;

    public NewRelicTelemetryAdapter(INewRelicTelemetryService newRelicService, ILogger<NewRelicTelemetryAdapter> logger)
    {
        _newRelicService = newRelicService;
        _logger = logger;
    }

    public void RecordServiceOrderEvent(Guid orderId, Guid customerId, string status, string action,
        IDictionary<string, object>? additionalAttributes = null)
    {
        var attributes = new Dictionary<string, object>
        {
            { "orderId", orderId.ToString() },
            { "customerId", customerId.ToString() },
            { "status", status },
            { "action", action },
            { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
        };

        if (additionalAttributes is not null)
        {
            foreach (var attr in additionalAttributes)
            {
                attributes.TryAdd(attr.Key, attr.Value);
            }
        }

        _newRelicService.RecordCustomEvent("ServiceOrder", attributes);
        _newRelicService.AddCustomAttribute("orderId", orderId.ToString());
        _newRelicService.AddCustomAttribute("customerId", customerId.ToString());
        _newRelicService.AddCustomAttribute("status", status);

        _logger.LogInformation(
            "ServiceOrder event: {Action} - OrderId: {OrderId}, CustomerId: {CustomerId}, Status: {Status}",
            action, orderId, customerId, status);
    }
}
