using backendORCinverisones.Domain.Entities;

namespace backendORCinverisones.Application.Interfaces.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByNameAsync(string name);

    /// <summary>
    /// Obtiene categorías activas filtradas por IDs (query optimizada en BD)
    /// </summary>
    Task<IEnumerable<Category>> GetActiveByIdsAsync(IEnumerable<int> ids);
}
