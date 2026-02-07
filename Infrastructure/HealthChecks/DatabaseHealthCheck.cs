using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace backendORCinverisones.Infrastructure.HealthChecks;

/// <summary>
/// ✅ HEALTH CHECK PARA BASE DE DATOS
/// Verifica conectividad y tiempo de respuesta de SQL Server
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(IConfiguration configuration, ILogger<DatabaseHealthCheck> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            await connection.OpenAsync(cancellationToken);
            
            // Verificar que podemos ejecutar una query simple
            using var command = new SqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync(cancellationToken);
            
            stopwatch.Stop();
            var responseTime = stopwatch.ElapsedMilliseconds;

            // Data adicional para diagnóstico
            var data = new Dictionary<string, object>
            {
                ["ResponseTimeMs"] = responseTime,
                ["ServerVersion"] = connection.ServerVersion,
                ["Database"] = connection.Database,
                ["State"] = connection.State.ToString()
            };

            // Degradar si el tiempo de respuesta es alto
            if (responseTime > 1000)
            {
                return HealthCheckResult.Degraded(
                    $"Database responding slowly ({responseTime}ms)",
                    data: data);
            }

            return HealthCheckResult.Healthy(
                $"Database is healthy ({responseTime}ms)",
                data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy(
                "Database connection failed",
                exception: ex);
        }
    }
}
