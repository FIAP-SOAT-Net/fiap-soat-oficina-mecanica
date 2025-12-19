namespace Fiap.Soat.SmartMechanicalWorkshop.Api.Shared.Services;

public interface INewRelicTelemetryService
{
    void RecordCustomEvent(string eventType, IDictionary<string, object> attributes);
    void RecordMetric(string name, double value);
    void AddCustomAttribute(string key, object value);
}
