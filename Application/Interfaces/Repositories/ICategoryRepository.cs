using backendORCinverisones.Domain.Entities;

namespace backendORCinverisones.Application.Interfaces.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByNameAsync(string name);
}
