namespace Fiap.Soat.SmartMechanicalWorkshop.Application.Shared.Services;

public interface ITelemetryService
{
    void RecordServiceOrderEvent(Guid orderId, Guid customerId, string status, string action,
        IDictionary<string, object>? additionalAttributes = null);
}
