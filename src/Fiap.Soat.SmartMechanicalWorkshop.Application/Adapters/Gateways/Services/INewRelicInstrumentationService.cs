namespace Fiap.Soat.SmartMechanicalWorkshop.Application.Adapters.Gateways.Services;

public interface INewRelicInstrumentationService
{
    void RecordServiceOrderEvent(
        string action,
        Guid orderId,
        string status,
        Guid customerId,
        TimeSpan? duration = null,
        Dictionary<string, object>? additionalAttributes = null);

    void RecordMetric(string metricName, double value);
    void NoticeError(Exception exception, Dictionary<string, object>? customAttributes = null);
}
