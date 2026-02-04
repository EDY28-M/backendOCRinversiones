using System.Diagnostics;
using backendORCinverisones.Application.Logging;

namespace backendORCinverisones.API.Middleware;

/// <summary>
/// ✅ MIDDLEWARE DE MONITOREO DE PERFORMANCE
/// Agrega headers de timing y loguea requests lentos
/// </summary>
public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
    private const int SLOW_REQUEST_THRESHOLD_MS = 1000;

    public PerformanceMonitoringMiddleware(RequestDelegate next, ILogger<PerformanceMonitoringMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        // Agregar header de inicio de procesamiento
        context.Response.Headers.Append("X-Request-Start", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Agregar headers de performance
            context.Response.Headers.Append("X-Response-Time-Ms", elapsedMs.ToString());
            context.Response.Headers.Append("X-Request-End", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());

            // Loguear requests lentos
            if (elapsedMs > SLOW_REQUEST_THRESHOLD_MS)
            {
                _logger.SlowQueryDetected(
                    $"{context.Request.Method} {context.Request.Path}",
                    elapsedMs);

                _logger.LogWarning(
                    "🐌 Request lento detectado: {Method} {Path} tomó {ElapsedMs}ms (Status: {StatusCode})",
                    context.Request.Method,
                    context.Request.Path,
                    elapsedMs,
                    context.Response.StatusCode);
            }

            // Loguear requests de API para análisis
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                _logger.LogDebug(
                    "API Request: {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    elapsedMs,
                    context.Response.StatusCode);
            }
        }
    }
}

/// <summary>
/// Extensiones para registrar el middleware
/// </summary>
public static class PerformanceMonitoringMiddlewareExtensions
{
    public static IApplicationBuilder UsePerformanceMonitoring(this IApplicationBuilder app)
    {
        return app.UseMiddleware<PerformanceMonitoringMiddleware>();
    }
}
