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
}
