using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using backendORCinverisones.Application.Services;

namespace backendORCinverisones.Infrastructure.HealthChecks;

/// <summary>
/// ✅ HEALTH CHECK PARA SERVICIO DE CACHÉ
/// Verifica el estado de MemoryCache y Redis
/// </summary>
public class CacheHealthCheck : IHealthCheck
{
    private readonly HybridCacheService _cacheService;
    private readonly IMemoryCache _memoryCache;

    public CacheHealthCheck(HybridCacheService cacheService, IMemoryCache memoryCache)
    {
        _cacheService = cacheService;
        _memoryCache = memoryCache;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var status = _cacheService.GetStatus();
            
            var data = new Dictionary<string, object>
            {
                ["MemoryCacheKeys"] = status.MemoryCacheKeys,
                ["RedisEnabled"] = status.RedisEnabled,
                ["RedisConnected"] = status.RedisConnected
            };

            // Test de escritura/lectura en caché
            var testKey = $"healthcheck_{Guid.NewGuid():N}";
            var testValue = DateTime.UtcNow.ToString("O");
            
            _memoryCache.Set(testKey, testValue, TimeSpan.FromMinutes(1));
            var retrieved = _memoryCache.TryGetValue(testKey, out var cachedValue);
            _memoryCache.Remove(testKey);

            if (!retrieved || !Equals(cachedValue, testValue))
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    "Cache read/write test failed",
                    data: data));
            }

            // Si Redis está habilitado pero no conectado, degradar
            if (status.RedisEnabled && !status.RedisConnected)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    "Redis is enabled but not connected. Falling back to MemoryCache only.",
                    data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                "Cache is healthy",
                data: data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Cache health check failed",
                exception: ex));
        }
    }
}
