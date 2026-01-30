using backendORCinverisones.Application.DTOs.Products;

namespace backendORCinverisones.Application.Interfaces.Services;

/// <summary>
/// Servicio de queries optimizadas con Dapper para alto rendimiento
/// </summary>
public interface IDapperQueryService
{
    /// <summary>
    /// Obtiene productos paginados usando Stored Procedure optimizado
    /// </summary>
    Task<(IEnumerable<ProductResponseDto> Items, int Total)> GetAvailableProductsPagedAsync(
        int page, int pageSize, string? searchTerm, int? categoryId, bool onlyActive);

    /// <summary>
    /// Verifica disponibilidad de código usando SP optimizado
    /// </summary>
    Task<bool> IsCodigoAvailableAsync(string codigo, int? excludeId = null);

    /// <summary>
    /// Obtiene códigos para generación (solo Codigo y CodigoComer)
    /// </summary>
    Task<IEnumerable<(string Codigo, string CodigoComer)>> GetCodigosForGenerationAsync();

    /// <summary>
    /// Obtiene estadísticas de productos para dashboards
    /// </summary>
    Task<Dictionary<string, int>> GetProductStatisticsAsync();
}
