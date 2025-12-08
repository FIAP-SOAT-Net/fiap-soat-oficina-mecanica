using Fiap.Soat.SmartMechanicalWorkshop.Application.Adapters.Gateways.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewRelic.Api.Agent;
using System.Diagnostics.CodeAnalysis;

namespace Fiap.Soat.SmartMechanicalWorkshop.Infrastructure.Services;

[ExcludeFromCodeCoverage]
public sealed class NewRelicInstrumentationService : INewRelicInstrumentationService
{
    private readonly bool _enabled;
    private readonly ILogger<NewRelicInstrumentationService> _logger;

    public NewRelicInstrumentationService(IConfiguration configuration, ILogger<NewRelicInstrumentationService> logger)
    {
        _logger = logger;
        _enabled = configuration.GetValue<bool>("NewRelic:Enabled", false);
    }

    public void RecordServiceOrderEvent(
        string action,
        Guid orderId,
        string status,
        Guid customerId,
        TimeSpan? duration = null,
        Dictionary<string, object>? additionalAttributes = null)
    {
        if (!_enabled)
        {
            return;
        }

        try
        {
            var attributes = new Dictionary<string, object>
            {
                { "action", action },
                { "orderId", orderId.ToString() },
                { "status", status },
                { "customerId", customerId.ToString() },
                { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
            };

            if (duration.HasValue)
            {
                attributes["durationMs"] = duration.Value.TotalMilliseconds;
            }

            if (additionalAttributes != null)
            {
                foreach (var kvp in additionalAttributes)
                {
                    attributes[kvp.Key] = kvp.Value;
                }
            }

            NewRelic.Api.Agent.NewRelic.RecordCustomEvent("ServiceOrder", attributes);

            _logger.LogDebug(
                "New Relic custom event recorded: ServiceOrder - Action: {Action}, OrderId: {OrderId}, Status: {Status}",
                action, orderId, status);
        }
        catch (Exception ex)
        {
            // Graceful degradation: log error but don't throw
            _logger.LogWarning(ex, "Failed to record New Relic custom event. Application continues normally.");
        }
    }

    public void RecordMetric(string metricName, double value)
    {
        if (!_enabled)
        {
            return;
        }

        try
        {
            NewRelic.Api.Agent.NewRelic.RecordMetric(metricName, (float) value);
            _logger.LogDebug("New Relic metric recorded: {MetricName} = {Value}", metricName, value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to record New Relic metric. Application continues normally.");
        }
    }

    public void NoticeError(Exception exception, Dictionary<string, object>? customAttributes = null)
    {
        if (!_enabled)
        {
            return;
        }

        try
        {
            NewRelic.Api.Agent.NewRelic.NoticeError(exception, customAttributes);
            _logger.LogDebug("New Relic error reported: {ExceptionType}", exception.GetType().Name);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to report error to New Relic. Application continues normally.");
        }
    }
}
