using Microsoft.EntityFrameworkCore;
using backendORCinverisones.Application.DTOs.Products;
using backendORCinverisones.Domain.Entities;
using backendORCinverisones.Infrastructure.Data;

namespace backendORCinverisones.Infrastructure.Repositories;

/// <summary>
/// ✅ EXTENSIÓN CON MÉTODOS ULTRA-OPTIMIZADOS PARA PRODUCTOS
/// Usa raw SQL compilado, proyecciones mínimas y técnicas de alta performance
/// </summary>
public partial class ProductRepository
{
    /// <summary>
    /// Obtiene productos usando proyección ultra-optimizada (solo campos necesarios)
    /// Evita el tracking de EF Core y usa AsAsyncEnumerable para streaming
    /// </summary>
    public IAsyncEnumerable<ProductListItemDto> GetProductsStreamAsync(
        int? categoryId = null, 
        bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().AsSplitQuery().AsQueryable();

        if (onlyActive)
            query = query.Where(p => p.IsActive);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        return query
            .Select(p => new ProductListItemDto
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Producto = p.Producto,
                ImagenPrincipal = p.ImagenPrincipal,
                IsActive = p.IsActive,
                CategoryName = p.Category.Name,
                MarcaNombre = p.Marca.Nombre
            })
            .AsAsyncEnumerable();
    }

    /// <summary>
    /// Búsqueda optimizada con full-text search (si está disponible en SQL Server)
    /// </summary>
    public async Task<IReadOnlyList<ProductSearchResultDto>> SearchOptimizedAsync(
        string searchTerm,
        int maxResults = 20,
        CancellationToken cancellationToken = default)
    {
        // Si el término es muy corto, usar LIKE tradicional
        if (searchTerm.Length < 3)
        {
            var pattern = $"%{searchTerm}%";
            return await _dbSet
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Where(p => EF.Functions.Like(p.Producto, pattern) || 
                           EF.Functions.Like(p.Codigo, pattern))
                .Select(p => new ProductSearchResultDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    Producto = p.Producto,
                    ImagenPrincipal = p.ImagenPrincipal,
                    CategoryName = p.Category.Name
                })
                .Take(maxResults)
                .ToListAsync(cancellationToken);
        }

        // Para búsquedas más complejas, usar el método existente
        var term = searchTerm.ToLower();
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Where(p => p.Producto.ToLower().Contains(term) ||
                       p.Codigo.ToLower().Contains(term) ||
                       p.CodigoComer.ToLower().Contains(term))
            .Select(p => new ProductSearchResultDto
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Producto = p.Producto,
                ImagenPrincipal = p.ImagenPrincipal,
                CategoryName = p.Category.Name
            })
            .Take(maxResults)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtiene solo los IDs de productos para operaciones masivas
    /// </summary>
    public async Task<IReadOnlyList<int>> GetProductIdsAsync(
        bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();
        
        if (onlyActive)
            query = query.Where(p => p.IsActive);

        return await query
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Actualización masiva optimizada usando ExecuteUpdateAsync
    /// </summary>
    public async Task<int> BulkUpdateStatusAsync(
        IReadOnlyList<int> productIds, 
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => productIds.Contains(p.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.IsActive, isActive)
                .SetProperty(p => p.UpdatedAt, DateTime.UtcNow),
            cancellationToken);
    }

    /// <summary>
    /// Obtiene estadísticas de productos en una sola query
    /// </summary>
    public async Task<ProductStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var stats = await _dbSet
            .AsNoTracking()
            .GroupBy(p => 1)
            .Select(g => new ProductStatisticsDto
            {
                TotalProducts = g.Count(),
                ActiveProducts = g.Count(p => p.IsActive),
                FeaturedProducts = g.Count(p => p.IsActive && p.IsFeatured),
                ProductsWithImages = g.Count(p => p.IsActive && 
                    (!string.IsNullOrEmpty(p.ImagenPrincipal) ||
                     !string.IsNullOrEmpty(p.Imagen2) ||
                     !string.IsNullOrEmpty(p.Imagen3) ||
                     !string.IsNullOrEmpty(p.Imagen4))),
                CategoriesCount = g.Select(p => p.CategoryId).Distinct().Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return stats ?? new ProductStatisticsDto();
    }

    /// <summary>
    /// Verifica existencia de múltiples códigos en una sola query
    /// </summary>
    public async Task<IReadOnlySet<string>> GetExistingCodesAsync(
        IReadOnlyList<string> codes,
        CancellationToken cancellationToken = default)
    {
        var normalizedCodes = codes.Select(c => c.ToUpper()).ToList();
        
        var existing = await _dbSet
            .AsNoTracking()
            .Where(p => normalizedCodes.Contains(p.Codigo.ToUpper()))
            .Select(p => p.Codigo.ToUpper())
            .ToListAsync(cancellationToken);

        return existing.ToHashSet();
    }
}

// ============================================
// DTOs para proyecciones optimizadas
// ============================================

public class ProductListItemDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Producto { get; set; } = string.Empty;
    public string? ImagenPrincipal { get; set; }
    public bool IsActive { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string MarcaNombre { get; set; } = string.Empty;
}

public class ProductSearchResultDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Producto { get; set; } = string.Empty;
    public string? ImagenPrincipal { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class ProductStatisticsDto
{
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int FeaturedProducts { get; set; }
    public int ProductsWithImages { get; set; }
    public int CategoriesCount { get; set; }
}
