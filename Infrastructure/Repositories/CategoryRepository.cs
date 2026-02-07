using Microsoft.EntityFrameworkCore;
using backendORCinverisones.Application.Interfaces.Repositories;
using backendORCinverisones.Domain.Entities;
using backendORCinverisones.Infrastructure.Data;

namespace backendORCinverisones.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Name == name);
    }

    /// <summary>
    /// ✅ OPTIMIZADO: Filtra en BD en lugar de cargar todos y filtrar en memoria
    /// </summary>
    public async Task<IEnumerable<Category>> GetActiveByIdsAsync(IEnumerable<int> ids)
    {
        var idsList = ids.ToList();
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.IsActive && idsList.Contains(c.Id))
            .OrderBy(c => c.Name)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task DeleteAllAsync()
    {
        await _dbSet.ExecuteDeleteAsync();
    }
}
