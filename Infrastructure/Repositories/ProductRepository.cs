using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;
using backendORCinverisones.Application.Interfaces.Repositories;
using backendORCinverisones.Domain.Entities;
using backendORCinverisones.Infrastructure.Data;

namespace backendORCinverisones.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetByIdWithCategoryAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Marca)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Marca)
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }

    public override async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Marca)
            .ToListAsync();
    }

    public async Task BulkInsertAsync(IList<Product> products)
    {
        var bulkConfig = new BulkConfig
        {
            SetOutputIdentity = true,
            BatchSize = 500,
            BulkCopyTimeout = 120
        };

        // Usar CreateExecutionStrategy para compatibilidad con EnableRetryOnFailure
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await _context.BulkInsertAsync(products, bulkConfig);
        });
    }

    // Specialized queries for performance optimization
    public async Task<(IEnumerable<Product> Items, int Total)> GetAvailableProductsPagedAsync(
        int page, int pageSize, string? searchTerm, int? categoryId,
        bool onlyWithImages, bool onlyActive)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Marca)
            .AsQueryable();

        // Filter by active status
        if (onlyActive)
        {
            query = query.Where(p => p.IsActive);
        }

        // Filter by images
        if (onlyWithImages)
        {
            query = query.Where(p =>
                !string.IsNullOrWhiteSpace(p.ImagenPrincipal) ||
                !string.IsNullOrWhiteSpace(p.Imagen2) ||
                !string.IsNullOrWhiteSpace(p.Imagen3) ||
                !string.IsNullOrWhiteSpace(p.Imagen4));
        }

        // Search filter - SQL LIKE
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(p =>
                p.Producto.ToLower().Contains(term) ||
                p.Codigo.ToLower().Contains(term) ||
                p.CodigoComer.ToLower().Contains(term) ||
                (p.Category != null && p.Category.Name.ToLower().Contains(term)) ||
                (p.Marca != null && p.Marca.Nombre.ToLower().Contains(term)));
        }

        // Category filter
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        // Total count BEFORE pagination
        var total = await query.CountAsync();

        // Paginate and execute
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<(IEnumerable<Product> Items, int Total)> GetPublicActiveProductsPagedAsync(
        int page, int pageSize, string? searchTerm, int? categoryId, int[]? brandIds)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Marca)
            .Where(p => p.IsActive)
            .Where(p =>
                !string.IsNullOrWhiteSpace(p.ImagenPrincipal) ||
                !string.IsNullOrWhiteSpace(p.Imagen2) ||
                !string.IsNullOrWhiteSpace(p.Imagen3) ||
                !string.IsNullOrWhiteSpace(p.Imagen4));

        // Search filter - SQL LIKE
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(p =>
                p.Producto.ToLower().Contains(term) ||
                p.Codigo.ToLower().Contains(term) ||
                p.CodigoComer.ToLower().Contains(term) ||
                (p.Category != null && p.Category.Name.ToLower().Contains(term)) ||
                (p.Marca != null && p.Marca.Nombre.ToLower().Contains(term)));
        }

        // Category filter
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        // Brand filter
        if (brandIds != null && brandIds.Length > 0)
        {
            query = query.Where(p => brandIds.Contains(p.MarcaId));
        }

        // Total count BEFORE pagination
        var total = await query.CountAsync();

        // Paginate and execute
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<bool> IsCodigoExistsAsync(string codigo, int? excludeProductId)
    {
        var query = _dbSet.AsNoTracking().Where(p => p.Codigo.ToUpper() == codigo.ToUpper());

        if (excludeProductId.HasValue)
        {
            query = query.Where(p => p.Id != excludeProductId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> IsCodigoComercialExistsAsync(string codigoComer, int? excludeProductId)
    {
        var query = _dbSet.AsNoTracking().Where(p => p.CodigoComer.ToUpper() == codigoComer.ToUpper());

        if (excludeProductId.HasValue)
        {
            query = query.Where(p => p.Id != excludeProductId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<int>> GetDistinctCategoryIdsWithActiveProductsAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Where(p =>
                !string.IsNullOrWhiteSpace(p.ImagenPrincipal) ||
                !string.IsNullOrWhiteSpace(p.Imagen2) ||
                !string.IsNullOrWhiteSpace(p.Imagen3) ||
                !string.IsNullOrWhiteSpace(p.Imagen4))
            .Select(p => p.CategoryId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<IEnumerable<int>> GetDistinctBrandIdsWithActiveProductsAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Where(p =>
                !string.IsNullOrWhiteSpace(p.ImagenPrincipal) ||
                !string.IsNullOrWhiteSpace(p.Imagen2) ||
                !string.IsNullOrWhiteSpace(p.Imagen3) ||
                !string.IsNullOrWhiteSpace(p.Imagen4))
            .Select(p => p.MarcaId)
            .Distinct()
            .ToListAsync();
    }
}
