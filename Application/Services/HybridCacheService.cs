using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using backendORCinverisones.Application.Interfaces.Services;
using backendORCinverisones.Application.Logging;

namespace backendORCinverisones.Application.Services;

/// <summary>
/// ✅ SERVICIO DE CACHÉ HÍBRIDA (Memory + Redis)
/// Implementa el patrón Cache-Aside con doble nivel:
/// 1. Nivel 1: MemoryCache (más rápido, por proceso)
/// 2. Nivel 2: Redis (distribuido, compartido entre instancias)
/// 
/// Beneficios:
/// - Mayor velocidad: datos frecuentes en memoria local
    /// - Escalabilidad: datos compartidos entre múltiples instancias del API
/// - Resiliencia: Redis como respaldo si la instancia se reinicia
/// </summary>
public class HybridCacheService : ICacheService, IDisposable
{
    private readonly IMemoryCache _memoryCache;
    private readonly IConnectionMultiplexer? _redis;
    private readonly IDatabase? _redisDb;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private readonly ConcurrentDictionary<string, byte> _cacheKeys = new();
    private readonly ILogger<HybridCacheService> _logger;
    private readonly bool _redisEnabled;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public HybridCacheService(
        IMemoryCache memoryCache, 
        ILogger<HybridCacheService> logger,
        IConfiguration configuration)
    {
        _memoryCache = memoryCache;
        _logger = logger;

        // Intentar conectar a Redis si está configurado
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            try
            {
                _redis = ConnectionMultiplexer.Connect(redisConnection);
                _redisDb = _redis.GetDatabase();
                _redisEnabled = true;
                logger.LogInformation("✅ Redis conectado correctamente");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "⚠️ No se pudo conectar a Redis. Usando solo MemoryCache.");
                _redisEnabled = false;
            }
        }
        else
        {
            _redisEnabled = false;
            logger.LogInformation("ℹ️ Redis no configurado. Usando solo MemoryCache.");
        }
    }

    /// <summary>
    /// Obtiene o crea un valor en caché (nivel 1: memoria, nivel 2: Redis)
    /// </summary>
    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null)
    {
        // NIVEL 1: Verificar MemoryCache primero (más rápido)
        if (_memoryCache.TryGetValue<T>(key, out var cachedValue))
        {
            _logger.CacheHit(key);
            return cachedValue;
        }

        // NIVEL 2: Verificar Redis si está habilitado
        if (_redisEnabled && _redisDb != null)
        {
            var redisValue = await _redisDb.StringGetAsync(key);
            if (!redisValue.IsNullOrEmpty)
            {
                try
                {
                    var redisData = JsonSerializer.Deserialize<T>(redisValue!, _jsonOptions);
                    if (redisData != null)
                    {
                        // Guardar en memoria local para próximas solicitudes
                        var memOptions = new MemoryCacheEntryOptions();
                        if (absoluteExpiration.HasValue)
                            memOptions.AbsoluteExpirationRelativeToNow = absoluteExpiration.Value;
                        _memoryCache.Set(key, redisData, memOptions);
                        
                        _logger.CacheHit(key);
                        return redisData;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error deserializando valor de Redis para clave {Key}", key);
                }
            }
        }

        _logger.CacheMiss(key);

        // LOCK: Prevenir cache stampede (múltiples requests calculando el mismo valor)
        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();

        try
        {
            // Double-check después de adquirir el lock
            if (_memoryCache.TryGetValue<T>(key, out cachedValue))
                return cachedValue;

            // Generar valor
            var value = await factory();
            if (value == null)
                return default;

            // Calcular expiración
            var expiration = absoluteExpiration ?? TimeSpan.FromMinutes(10);

            // Guardar en NIVEL 1 (MemoryCache)
            var memoryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiration)
                .RegisterPostEvictionCallback((evictedKey, _, reason, _) =>
                {
                    _logger.LogDebug("Cache eviction: {Key}, Reason: {Reason}", evictedKey, reason);
                });

            _memoryCache.Set(key, value, memoryOptions);
            _cacheKeys.TryAdd(key, 0);

            // Guardar en NIVEL 2 (Redis) si está habilitado
            if (_redisEnabled && _redisDb != null)
            {
                try
                {
                    var serialized = JsonSerializer.Serialize(value, _jsonOptions);
                    await _redisDb.StringSetAsync(key, serialized, expiration);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error guardando en Redis para clave {Key}", key);
                }
            }

            return value;
        }
        finally
        {
            semaphore.Release();
            _locks.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Elimina una clave específica de ambos niveles de caché
    /// </summary>
    public void Remove(string key)
    {
        _memoryCache.Remove(key);
        _cacheKeys.TryRemove(key, out _);

        if (_redisEnabled && _redisDb != null)
        {
            _ = _redisDb.KeyDeleteAsync(key);
        }

        _logger.LogDebug("Cache removed: {Key}", key);
    }

    /// <summary>
    /// Elimina todas las claves que comienzan con el prefijo especificado
    /// </summary>
    public void RemoveByPrefix(string prefix)
    {
        // Eliminar de MemoryCache
        var keysToRemove = _cacheKeys.Keys.Where(k => k.StartsWith(prefix)).ToList();
        foreach (var key in keysToRemove)
        {
            _memoryCache.Remove(key);
            _cacheKeys.TryRemove(key, out _);
        }

        // Eliminar de Redis (usar SCAN para no bloquear)
        if (_redisEnabled && _redis != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var server = _redis.GetServer(_redis.GetEndPoints().First());
                    var keys = server.Keys(pattern: $"{prefix}*", pageSize: 100).ToArray();
                    if (keys.Length > 0)
                    {
                        await _redisDb!.KeyDeleteAsync(keys);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error removiendo claves con prefijo {Prefix} de Redis", prefix);
                }
            });
        }

        _logger.CacheInvalidated(prefix);
    }

    /// <summary>
    /// Invalida toda la caché (usar con precaución)
    /// </summary>
    public void InvalidateAll()
    {
        // Limpiar MemoryCache
        foreach (var key in _cacheKeys.Keys.ToList())
        {
            _memoryCache.Remove(key);
        }
        _cacheKeys.Clear();

        // Limpiar Redis
        if (_redisEnabled && _redis != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var server = _redis.GetServer(_redis.GetEndPoints().First());
                    await server.FlushDatabaseAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error limpiando Redis");
                }
            });
        }

        _logger.LogInformation("🗑️ Toda la caché ha sido invalidada");
    }

    /// <summary>
    /// Obtiene el estado actual de la caché (para health checks)
    /// </summary>
    public CacheStatus GetStatus()
    {
        return new CacheStatus
        {
            MemoryCacheKeys = _cacheKeys.Count,
            RedisEnabled = _redisEnabled,
            RedisConnected = _redis?.IsConnected ?? false
        };
    }

    public void Dispose()
    {
        _redis?.Dispose();
        foreach (var semaphore in _locks.Values)
        {
            semaphore.Dispose();
        }
    }
}

/// <summary>
/// Estado actual del servicio de caché
/// </summary>
public class CacheStatus
{
    public int MemoryCacheKeys { get; set; }
    public bool RedisEnabled { get; set; }
    public bool RedisConnected { get; set; }
}
