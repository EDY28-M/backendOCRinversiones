using backendORCinverisones.Domain.Entities;

namespace backendORCinverisones.Application.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByIdWithCategoryAsync(int id);
    Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId);
    Task BulkInsertAsync(IList<Product> products);

    // Specialized queries for performance optimization
    Task<(IEnumerable<Product> Items, int Total)> GetAvailableProductsPagedAsync(
        int page, int pageSize, string? searchTerm, int? categoryId,
        bool onlyWithImages, bool onlyActive);

    Task<(IEnumerable<Product> Items, int Total)> GetPublicActiveProductsPagedAsync(
        int page, int pageSize, string? searchTerm, int? categoryId, int[]? brandIds);

    Task<bool> IsCodigoExistsAsync(string codigo, int? excludeProductId);
    Task<bool> IsCodigoComercialExistsAsync(string codigoComer, int? excludeProductId);

    Task<IEnumerable<int>> GetDistinctCategoryIdsWithActiveProductsAsync();
    Task<IEnumerable<int>> GetDistinctBrandIdsWithActiveProductsAsync();
}
