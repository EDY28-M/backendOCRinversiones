using backendORCinverisones.Application.DTOs.Products;
using backendORCinverisones.Domain.Entities;

namespace backendORCinverisones.Application.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    /// <summary>
    /// Lista optimizada para admin: una sola consulta con proyección (sin cargar entidades completas).
    /// </summary>
    Task<IReadOnlyList<ProductResponseDto>> GetAllForListAsync();
    Task<Product?> GetByIdWithCategoryAsync(int id);
    Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId);
    Task BulkInsertAsync(IList<Product> products);

    // Specialized queries for performance optimization
    Task<(IEnumerable<Product> Items, int Total)> GetAvailableProductsPagedAsync(
        int page, int pageSize, string? searchTerm, int? categoryId,
        bool onlyWithImages, bool onlyActive);

    Task<(IEnumerable<Product> Items, int Total)> GetPublicActiveProductsPagedAsync(
        int page, int pageSize, string? searchTerm, int? categoryId, int[]? brandIds);

    Task<(IEnumerable<Product> Items, int Total)> GetPublicFeaturedProductsPagedAsync(
        int page, int pageSize);

    Task<bool> IsCodigoExistsAsync(string codigo, int? excludeProductId);

    Task<bool> IsCodigoComercialExistsAsync(string codigoComer, int? excludeProductId);

    /// <summary>
    /// Obtiene solo Codigo y CodigoComer para generación de códigos (una sola lectura, sin includes).
    /// </summary>
    Task<IReadOnlyList<(string Codigo, string CodigoComer)>> GetCodigosForGenerationAsync();

    Task UpdateStatusAsync(int id, bool isActive);
    Task UpdateFeaturedAsync(int id, bool isFeatured);

    /// <summary>
    /// Cuenta los productos destacados activos (para validar límite de 9).
    /// </summary>
    Task<int> CountFeaturedActiveProductsAsync();

    Task<IEnumerable<int>> GetDistinctCategoryIdsWithActiveProductsAsync();
    Task<IEnumerable<int>> GetDistinctBrandIdsWithActiveProductsAsync();

    Task DeleteAllAsync();
}
