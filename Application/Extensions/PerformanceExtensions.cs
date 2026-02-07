using System.Diagnostics;

namespace backendORCinverisones.Application.Extensions;

/// <summary>
/// ✅ EXTENSIONES DE PERFORMANCE
/// Métricas y diagnósticos de alta precisión
/// </summary>
public static class PerformanceExtensions
{
    /// <summary>
    /// Ejecuta una función y mide su tiempo de ejecución
    /// </summary>
    public static async Task<(T Result, TimeSpan Duration)> MeasureAsync<T>(
        this ILogger logger,
        Func<Task<T>> operation,
        string operationName,
        LogLevel logLevel = LogLevel.Debug)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await operation();
            stopwatch.Stop();
            
            logger.Log(logLevel, 
                "Operation {OperationName} completed in {ElapsedMs}ms",
                operationName, stopwatch.ElapsedMilliseconds);
            
            return (result, stopwatch.Elapsed);
        }
        catch (Exception)
        {
            stopwatch.Stop();
            logger.LogWarning(
                "Operation {OperationName} failed after {ElapsedMs}ms",
                operationName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Ejecuta una acción y mide su tiempo de ejecución
    /// </summary>
    public static async Task<TimeSpan> MeasureAsync(
        this ILogger logger,
        Func<Task> operation,
        string operationName,
        LogLevel logLevel = LogLevel.Debug)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await operation();
            stopwatch.Stop();
            
            logger.Log(logLevel, 
                "Operation {OperationName} completed in {ElapsedMs}ms",
                operationName, stopwatch.ElapsedMilliseconds);
            
            return stopwatch.Elapsed;
        }
        catch (Exception)
        {
            stopwatch.Stop();
            logger.LogWarning(
                "Operation {OperationName} failed after {ElapsedMs}ms",
                operationName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Ejecuta una función síncrona y mide su tiempo
    /// </summary>
    public static (T Result, TimeSpan Duration) Measure<T>(
        this ILogger logger,
        Func<T> operation,
        string operationName,
        LogLevel logLevel = LogLevel.Debug)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = operation();
            stopwatch.Stop();
            
            logger.Log(logLevel, 
                "Operation {OperationName} completed in {ElapsedMs}ms",
                operationName, stopwatch.ElapsedMilliseconds);
            
            return (result, stopwatch.Elapsed);
        }
        catch (Exception)
        {
            stopwatch.Stop();
            logger.LogWarning(
                "Operation {OperationName} failed after {ElapsedMs}ms",
                operationName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Agrega información de performance al HttpContext
    /// </summary>
    public static void SetPerformanceMetrics(
        this HttpContext context,
        string key,
        object value)
    {
        if (context.Items["PerformanceMetrics"] is not Dictionary<string, object> metrics)
        {
            metrics = new Dictionary<string, object>();
            context.Items["PerformanceMetrics"] = metrics;
        }
        metrics[key] = value;
    }

    /// <summary>
    /// Obtiene métricas de performance del request actual
    /// </summary>
    public static Dictionary<string, object>? GetPerformanceMetrics(this HttpContext context)
    {
        return context.Items["PerformanceMetrics"] as Dictionary<string, object>;
    }
}

/// <summary>
/// Atributo para marcar métodos que deben ser monitoreados
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class PerformanceMonitorAttribute : Attribute
{
    public string? OperationName { get; set; }
    public int SlowThresholdMs { get; set; } = 500;
}
