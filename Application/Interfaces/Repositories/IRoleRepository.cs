using backendORCinverisones.Domain.Entities;

namespace backendORCinverisones.Application.Interfaces.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string name);
}
