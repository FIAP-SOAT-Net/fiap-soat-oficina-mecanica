using NewRelic.Api.Agent;
using System.Diagnostics.CodeAnalysis;

namespace Fiap.Soat.SmartMechanicalWorkshop.Api.Shared.Services;

[ExcludeFromCodeCoverage]
public sealed class NewRelicTelemetryService : INewRelicTelemetryService
{
    private readonly bool _isEnabled;
    private readonly ILogger<NewRelicTelemetryService> _logger;

    public NewRelicTelemetryService(IConfiguration configuration, ILogger<NewRelicTelemetryService> logger)
    {
        _logger = logger;
        _isEnabled = configuration.GetValue<bool>("NewRelic:Enabled", false);

        if (!_isEnabled)
        {
            _logger.LogWarning("New Relic telemetry is disabled. Custom events will not be recorded.");
        }
    }

    public void RecordCustomEvent(string eventType, IDictionary<string, object> attributes)
    {
        if (!_isEnabled)
        {
            return;
        }

        try
        {
            Task.Run(() =>
            {
                try
                {
                    NewRelic.Api.Agent.NewRelic.RecordCustomEvent(eventType, attributes);
                    _logger.LogDebug("Custom event recorded: {EventType} with {AttributeCount} attributes",
                        eventType, attributes.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to record custom event {EventType}", eventType);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initiate custom event recording for {EventType}", eventType);
        }
    }

    public void RecordMetric(string name, double value)
    {
        if (!_isEnabled)
        {
            return;
        }

        try
        {
            Task.Run(() =>
            {
                try
                {
                    NewRelic.Api.Agent.NewRelic.RecordMetric(name, (float) value);
                    _logger.LogDebug("Metric recorded: {MetricName} = {Value}", name, value);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to record metric {MetricName}", name);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initiate metric recording for {MetricName}", name);
        }
    }

    public void AddCustomAttribute(string key, object value)
    {
        if (!_isEnabled)
        {
            return;
        }

        try
        {
            IAgent agent = NewRelic.Api.Agent.NewRelic.GetAgent();
            ITransaction transaction = agent.CurrentTransaction;
            transaction.AddCustomAttribute(key, value);

            _logger.LogDebug("Custom attribute added: {Key} = {Value}", key, value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to add custom attribute {Key}", key);
        }
    }

    public void NoticeError(Exception exception, IDictionary<string, object>? attributes = null)
    {
        if (!_isEnabled)
        {
            return;
        }

        try
        {
            Task.Run(() =>
            {
                try
                {
                    NewRelic.Api.Agent.NewRelic.NoticeError(exception, attributes);
                    _logger.LogDebug("Error noticed in New Relic: {ErrorMessage}", exception.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to notice error in New Relic");
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initiate error notice");
        }
    }

    public void RecordServiceOrderMetrics(string status, TimeSpan duration, int servicesCount = 0)
    {
        if (!_isEnabled)
        {
            return;
        }

        try
        {
            // Record duration by status
            RecordMetric($"Custom/ServiceOrder/Duration/{status}", duration.TotalMilliseconds);

            // Record count by status
            RecordMetric($"Custom/ServiceOrder/Count/{status}", 1);

            if (servicesCount > 0)
            {
                RecordMetric("Custom/ServiceOrder/ServicesPerOrder", servicesCount);
            }

            _logger.LogDebug("Service order metrics recorded for status {Status}", status);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to record service order metrics");
        }
    }
}
