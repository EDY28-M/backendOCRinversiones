using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using backendORCinverisones.Application.Logging;

namespace backendORCinverisones.Infrastructure.Data;

/// <summary>
/// ✅ INTERCEPTOR DE EF CORE PARA MONITOREO DE PERFORMANCE
/// Detecta queries lentas y registra métricas de ejecución
/// </summary>
public class DatabasePerformanceInterceptor : DbCommandInterceptor
{
    private readonly ILogger<DatabasePerformanceInterceptor> _logger;
    private const int SLOW_QUERY_THRESHOLD_MS = 500; // Umbral para considerar una query lenta

    public DatabasePerformanceInterceptor(ILogger<DatabasePerformanceInterceptor> logger)
    {
        _logger = logger;
    }

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        LogPerformance(eventData, command.CommandText);
        return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override async ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        LogPerformance(eventData, command.CommandText);
        return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override async ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        LogPerformance(eventData, command.CommandText);
        return await base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void LogPerformance(CommandExecutedEventData eventData, string commandText)
    {
        var duration = eventData.Duration.TotalMilliseconds;
        
        // Extraer el nombre/tipo de query para logging
        var queryName = ExtractQueryName(commandText);

        if (duration > SLOW_QUERY_THRESHOLD_MS)
        {
            _logger.SlowQueryDetected(queryName, (long)duration);
            
            // Log detallado para queries muy lentas (> 2 segundos)
            if (duration > 2000)
            {
                _logger.LogWarning(
                    "Query muy lenta ({Duration}ms): {Query}",
                    duration,
                    commandText.Length > 200 ? commandText[..200] + "..." : commandText
                );
            }
        }
        else
        {
            _logger.LogDebug("Query {QueryName} ejecutada en {Duration}ms", queryName, duration);
        }
    }

    private static string ExtractQueryName(string commandText)
    {
        // Intentar extraer el nombre de la tabla principal o tipo de operación
        var text = commandText.Trim().ToUpperInvariant();
        
        if (text.StartsWith("SELECT"))
        {
            // Busar FROM o JOIN para identificar la tabla
            var fromIndex = text.IndexOf("FROM ");
            if (fromIndex >= 0)
            {
                var afterFrom = text.Substring(fromIndex + 5).Trim();
                var tableName = afterFrom.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (!string.IsNullOrEmpty(tableName))
                    return $"SELECT {tableName}";
            }
            return "SELECT";
        }
        
        if (text.StartsWith("INSERT")) return "INSERT";
        if (text.StartsWith("UPDATE")) return "UPDATE";
        if (text.StartsWith("DELETE")) return "DELETE";
        if (text.StartsWith("EXEC ")) return "STORED_PROCEDURE";
        
        return "UNKNOWN";
    }
}
