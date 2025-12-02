using Fiap.Soat.SmartMechanicalWorkshop.Application.Adapters.Gateways.Services;

namespace Fiap.Soat.SmartMechanicalWorkshop.Tests.Shared.Factories;

public sealed class FakeNewRelicInstrumentationService : INewRelicInstrumentationService
{
    public void RecordServiceOrderEvent(
        string action,
        Guid orderId,
        string status,
        Guid customerId,
        TimeSpan? duration = null,
        Dictionary<string, object>? additionalAttributes = null)
    {
        // No-op for tests
    }

    public void RecordMetric(string metricName, double value)
    {
        // No-op for tests
    }

    public void NoticeError(Exception exception, Dictionary<string, object>? customAttributes = null)
    {
        // No-op for tests
    }
}
