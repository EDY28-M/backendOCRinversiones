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
}
