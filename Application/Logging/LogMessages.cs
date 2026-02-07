using Microsoft.Extensions.Logging;

namespace backendORCinverisones.Application.Logging;

/// <summary>
/// ✅ HIGH-PERFORMANCE LOGGING con LoggerMessage
/// Reduce allocations de memoria al evitar interpolación de strings en logs frecuentes
/// https://learn.microsoft.com/en-us/dotnet/core/extensions/high-performance-logging
/// </summary>
public static partial class LogMessages
{
    // ============================================
    // Product Logs
    // ============================================
    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, 
        Message = "Producto {Codigo} creado por {Usuario}")]
    public static partial void ProductCreated(this ILogger logger, string codigo, string? usuario);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, 
        Message = "Producto {Codigo} actualizado por {Usuario}")]
    public static partial void ProductUpdated(this ILogger logger, string codigo, string? usuario);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Information, 
        Message = "Producto {Codigo} eliminado por {Usuario}")]
    public static partial void ProductDeleted(this ILogger logger, string codigo, string? usuario);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Warning, 
        Message = "Intento de crear producto con código duplicado: {Codigo}")]
    public static partial void DuplicateProductCodeAttempt(this ILogger logger, string codigo);

    [LoggerMessage(EventId = 1005, Level = LogLevel.Information, 
        Message = "Bulk import completado: {Importados} importados, {Fallidos} fallidos, {Duplicados} duplicados")]
    public static partial void BulkImportCompleted(this ILogger logger, int importados, int fallidos, int duplicados);

    // ============================================
    // Authentication Logs
    // ============================================
    [LoggerMessage(EventId = 2001, Level = LogLevel.Information, 
        Message = "Usuario {Username} ha iniciado sesión")]
    public static partial void UserLoginSuccess(this ILogger logger, string username);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Warning, 
        Message = "Intento de login fallido para usuario: {Username}")]
    public static partial void UserLoginFailed(this ILogger logger, string username);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Information, 
        Message = "Usuario {Username} cerró sesión")]
    public static partial void UserLogout(this ILogger logger, string username);

    // ============================================
    // User Management Logs
    // ============================================
    [LoggerMessage(EventId = 3001, Level = LogLevel.Information, 
        Message = "Usuario {Username} creado por {CreatedBy}")]
    public static partial void UserCreated(this ILogger logger, string username, string? createdBy);

    [LoggerMessage(EventId = 3002, Level = LogLevel.Information, 
        Message = "Usuario {Username} actualizado por {UpdatedBy}")]
    public static partial void UserUpdated(this ILogger logger, string username, string? updatedBy);

    [LoggerMessage(EventId = 3003, Level = LogLevel.Information, 
        Message = "Usuario {Username} eliminado por {DeletedBy}")]
    public static partial void UserDeleted(this ILogger logger, string username, string? deletedBy);

    // ============================================
    // Performance Logs
    // ============================================
    [LoggerMessage(EventId = 4001, Level = LogLevel.Warning, 
        Message = "Query lenta detectada: {QueryName} tomó {ElapsedMs}ms")]
    public static partial void SlowQueryDetected(this ILogger logger, string queryName, long elapsedMs);

    [LoggerMessage(EventId = 4002, Level = LogLevel.Information, 
        Message = "Cache hit para clave: {CacheKey}")]
    public static partial void CacheHit(this ILogger logger, string cacheKey);

    [LoggerMessage(EventId = 4003, Level = LogLevel.Information, 
        Message = "Cache miss para clave: {CacheKey}")]
    public static partial void CacheMiss(this ILogger logger, string cacheKey);

    [LoggerMessage(EventId = 4004, Level = LogLevel.Information, 
        Message = "Cache invalidado para prefijo: {Prefix}")]
    public static partial void CacheInvalidated(this ILogger logger, string prefix);

    // ============================================
    // Database Logs
    // ============================================
    [LoggerMessage(EventId = 5001, Level = LogLevel.Error, 
        Message = "Error de conexión a base de datos: {Error}")]
    public static partial void DatabaseConnectionError(this ILogger logger, string error);

    [LoggerMessage(EventId = 5002, Level = LogLevel.Warning, 
        Message = "Retry de operación de base de datos. Intento {Attempt} de {MaxRetries}")]
    public static partial void DatabaseRetryAttempt(this ILogger logger, int attempt, int maxRetries);

    // ============================================
    // Security Logs
    // ============================================
    [LoggerMessage(EventId = 6001, Level = LogLevel.Warning, 
        Message = "Rate limit excedido para IP: {IpAddress}, Endpoint: {Endpoint}")]
    public static partial void RateLimitExceeded(this ILogger logger, string ipAddress, string endpoint);

    [LoggerMessage(EventId = 6002, Level = LogLevel.Warning, 
        Message = "Acceso no autorizado intentado por {Usuario} a {Recurso}")]
    public static partial void UnauthorizedAccessAttempt(this ILogger logger, string? usuario, string recurso);
}
