using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using backendORCinverisones.Application.Interfaces.Services;
using backendORCinverisones.Application.Logging;

namespace backendORCinverisones.Application.Services;

/// <summary>
/// Servicio de caché en memoria con patrón cache-aside.
/// </summary>
public class HybridCacheService : ICacheService, IDisposable
{
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private readonly ConcurrentDictionary<string, byte> _cacheKeys = new();
    private readonly ILogger<HybridCacheService> _logger;

    public HybridCacheService(
        IMemoryCache memoryCache,
        ILogger<HybridCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null)
    {
        if (_memoryCache.TryGetValue<T>(key, out var cachedValue))
        {
            _logger.CacheHit(key);
            return cachedValue;
        }

        _logger.CacheMiss(key);

        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();

        try
        {
            if (_memoryCache.TryGetValue<T>(key, out cachedValue))
            {
                _logger.CacheHit(key);
                return cachedValue;
            }

            var value = await factory();
            if (value == null)
            {
                return default;
            }

            var expiration = absoluteExpiration ?? TimeSpan.FromMinutes(10);
            var memoryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiration)
                .RegisterPostEvictionCallback((evictedKey, _, reason, _) =>
                {
                    if (evictedKey is string keyToRemove)
                    {
                        _cacheKeys.TryRemove(keyToRemove, out _);
                    }

                    _logger.LogDebug("Cache eviction: {Key}, Reason: {Reason}", evictedKey, reason);
                });

            _memoryCache.Set(key, value, memoryOptions);
            _cacheKeys.TryAdd(key, 0);

            return value;
        }
        finally
        {
            semaphore.Release();
            if (semaphore.CurrentCount == 1)
            {
                _locks.TryRemove(key, out _);
            }
        }
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
        _cacheKeys.TryRemove(key, out _);
        _logger.LogDebug("Cache removed: {Key}", key);
    }

    public void RemoveByPrefix(string prefix)
    {
        var keysToRemove = _cacheKeys.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.Ordinal))
            .ToList();

        foreach (var key in keysToRemove)
        {
            Remove(key);
        }

        _logger.CacheInvalidated(prefix);
    }

    public void InvalidateAll()
    {
        var allKeys = _cacheKeys.Keys.ToList();
        foreach (var key in allKeys)
        {
            _memoryCache.Remove(key);
        }

        _cacheKeys.Clear();
        _logger.LogInformation("Toda la caché en memoria ha sido invalidada");
    }

    public CacheStatus GetStatus()
    {
        return new CacheStatus
        {
            MemoryCacheKeys = _cacheKeys.Count
        };
    }

    public void Dispose()
    {
        foreach (var semaphore in _locks.Values)
        {
            semaphore.Dispose();
        }
        _locks.Clear();
    }
}

public class CacheStatus
{
    public int MemoryCacheKeys { get; set; }
}
