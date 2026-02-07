using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using backendORCinverisones.Application.DTOs.Products;
using backendORCinverisones.Application.Interfaces.Services;

namespace backendORCinverisones.Application.Services;

/// <summary>
/// ✅ SERVICIO DE ALTO RENDIMIENTO con Dapper
/// Ejecuta stored procedures y queries raw SQL optimizadas
/// Hasta 10x más rápido que EF Core para queries complejas
/// </summary>
public class DapperQueryService : IDapperQueryService
{
    private readonly string _connectionString;

    public DapperQueryService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task<(IEnumerable<ProductResponseDto> Items, int Total)> GetAvailableProductsPagedAsync(
        int page, int pageSize, string? searchTerm, int? categoryId, bool onlyActive)
    {
        using var connection = new SqlConnection(_connectionString);

        var parameters = new DynamicParameters();
        parameters.Add("@Page", page);
        parameters.Add("@PageSize", pageSize);
        parameters.Add("@SearchTerm", searchTerm);
        parameters.Add("@CategoryId", categoryId);
        parameters.Add("@OnlyActive", onlyActive);

        // Ejecuta SP optimizado con índices compuestos
        var results = await connection.QueryAsync<ProductResponseDtoWithCount>(
            "SP_GetAvailableProductsPaged",
            parameters,
            commandType: CommandType.StoredProcedure,
            commandTimeout: 30
        );

        var items = results.ToList();
        var total = items.FirstOrDefault()?.TotalCount ?? 0;

        return (items, total);
    }

    public async Task<bool> IsCodigoAvailableAsync(string codigo, int? excludeId = null)
    {
        using var connection = new SqlConnection(_connectionString);

        var parameters = new DynamicParameters();
        parameters.Add("@Codigo", codigo);
        parameters.Add("@ExcludeId", excludeId);

        var result = await connection.QuerySingleAsync<bool>(
            "SP_IsCodigoAvailable",
            parameters,
            commandType: CommandType.StoredProcedure,
            commandTimeout: 5
        );

        return result;
    }

    public async Task<IEnumerable<(string Codigo, string CodigoComer)>> GetCodigosForGenerationAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        // Query raw optimizada - solo 2 columnas
        var results = await connection.QueryAsync<CodigosPair>(
            "SELECT Codigo, CodigoComer FROM Products WITH (NOLOCK)",
            commandTimeout: 10
        );

        return results.Select(r => (r.Codigo, r.CodigoComer));
    }

    public async Task<Dictionary<string, int>> GetProductStatisticsAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        var results = await connection.QueryAsync<StatisticRow>(
            "SP_GetProductStatistics",
            commandType: CommandType.StoredProcedure,
            commandTimeout: 10
        );

        return results.ToDictionary(r => r.Metric, r => r.Value);
    }

    // DTO auxiliares para Dapper mapping
    private class ProductResponseDtoWithCount : ProductResponseDto
    {
        public int TotalCount { get; set; }
    }

    private class CodigosPair
    {
        public string Codigo { get; set; } = string.Empty;
        public string CodigoComer { get; set; } = string.Empty;
    }

    private class StatisticRow
    {
        public string Metric { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
