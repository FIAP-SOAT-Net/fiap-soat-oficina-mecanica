using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Fiap.Soat.SmartMechanicalWorkshop.Api.Shared.HealthChecks;

public sealed class MemoryHealthCheck : IHealthCheck
{
    private readonly long _threshold;

    public MemoryHealthCheck(long thresholdInBytes = 1024L * 1024L * 1024L)
    {
        _threshold = thresholdInBytes;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var allocated = GC.GetTotalMemory(forceFullCollection: false);
        var data = new Dictionary<string, object>
        {
            { "allocated", allocated },
            { "threshold", _threshold },
            { "gen0Collections", GC.CollectionCount(0) },
            { "gen1Collections", GC.CollectionCount(1) },
            { "gen2Collections", GC.CollectionCount(2) }
        };

        var status = allocated < _threshold ? HealthStatus.Healthy : HealthStatus.Degraded;

        return Task.FromResult(new HealthCheckResult(
            status,
            description: $"Memory usage: {allocated / 1024 / 1024} MB",
            data: data));
    }
}
