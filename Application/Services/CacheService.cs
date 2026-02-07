using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using backendORCinverisones.Application.Interfaces.Services;

namespace backendORCinverisones.Application.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private readonly ConcurrentDictionary<string, byte> _cacheKeys = new();

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory,
        TimeSpan? absoluteExpiration = null)
    {
        if (_cache.TryGetValue<T>(key, out var cached))
            return cached;

        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        try
        {
            if (_cache.TryGetValue<T>(key, out cached))
                return cached;

            var value = await factory();
            var options = new MemoryCacheEntryOptions();
            if (absoluteExpiration.HasValue)
                options.AbsoluteExpirationRelativeToNow = absoluteExpiration.Value;

            _cache.Set(key, value, options);
            _cacheKeys.TryAdd(key, 0);
            return value;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        _cacheKeys.TryRemove(key, out _);
    }

    public void RemoveByPrefix(string prefix)
    {
        var keysToRemove = _cacheKeys.Keys.Where(k => k.StartsWith(prefix)).ToList();
        foreach (var key in keysToRemove)
        {
            Remove(key);
        }
    }
}
