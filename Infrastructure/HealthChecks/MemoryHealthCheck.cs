using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace backendORCinverisones.Infrastructure.HealthChecks;

/// <summary>
/// ✅ HEALTH CHECK PARA MEMORIA DEL SISTEMA
/// Monitorea uso de memoria y generación de GC
/// </summary>
public class MemoryHealthCheck : IHealthCheck
{
    private readonly ILogger<MemoryHealthCheck> _logger;
    
    // Umbrales de memoria (en MB)
    private const long HEALTHY_THRESHOLD_MB = 512;
    private const long DEGRADED_THRESHOLD_MB = 1024;

    public MemoryHealthCheck(ILogger<MemoryHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var workingSetMB = process.WorkingSet64 / (1024 * 1024);
            var gcMemoryMB = GC.GetTotalMemory(false) / (1024 * 1024);
            
            // Obtener estadísticas de GC
            var gcCollections = new[]
            {
                GC.CollectionCount(0),
                GC.CollectionCount(1),
                GC.CollectionCount(2)
            };

            var data = new Dictionary<string, object>
            {
                ["WorkingSetMB"] = workingSetMB,
                ["GCMemoryMB"] = gcMemoryMB,
                ["GCGen0Collections"] = gcCollections[0],
                ["GCGen1Collections"] = gcCollections[1],
                ["GCGen2Collections"] = gcCollections[2],
                ["ThreadCount"] = process.Threads.Count,
                ["HandleCount"] = process.HandleCount,
                ["StartTime"] = process.StartTime.ToUniversalTime().ToString("O"),
                ["TotalProcessorTime"] = process.TotalProcessorTime.ToString()
            };

            // Determinar estado basado en uso de memoria
            if (workingSetMB > DEGRADED_THRESHOLD_MB)
            {
                _logger.LogWarning(
                    "Memory usage is high: {WorkingSetMB}MB working set, {GCMemoryMB}MB GC heap",
                    workingSetMB, gcMemoryMB);
                    
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Memory usage is critical: {workingSetMB}MB",
                    data: data));
            }

            if (workingSetMB > HEALTHY_THRESHOLD_MB)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Memory usage is elevated: {workingSetMB}MB",
                    data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                $"Memory is healthy: {workingSetMB}MB",
                data: data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Memory health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Memory health check failed",
                exception: ex));
        }
    }
}
