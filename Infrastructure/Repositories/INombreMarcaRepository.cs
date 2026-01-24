using backendORCinverisones.Domain.Entities;

namespace backendORCinverisones.Infrastructure.Repositories;

public interface INombreMarcaRepository
{
    Task<IEnumerable<NombreMarca>> GetAllAsync();
    Task<NombreMarca?> GetByIdAsync(int id);
    Task<NombreMarca> CreateAsync(NombreMarca nombreMarca);
    Task<NombreMarca> UpdateAsync(NombreMarca nombreMarca);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsByNameAsync(string nombreMarca, int? excludeId = null);
}
