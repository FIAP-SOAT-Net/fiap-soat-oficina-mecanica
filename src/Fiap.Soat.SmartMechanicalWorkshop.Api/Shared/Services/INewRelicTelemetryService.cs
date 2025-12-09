namespace Fiap.Soat.SmartMechanicalWorkshop.Api.Shared.Services;

public interface INewRelicTelemetryService
{
    void RecordCustomEvent(string eventType, IDictionary<string, object> attributes);
    void RecordMetric(string name, double value);
    void AddCustomAttribute(string key, object value);
    void NoticeError(Exception exception, IDictionary<string, object>? attributes = null);
    void RecordServiceOrderMetrics(string status, TimeSpan duration, int servicesCount = 0);
}
