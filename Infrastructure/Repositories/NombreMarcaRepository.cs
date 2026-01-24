using backendORCinverisones.Domain.Entities;
using backendORCinverisones.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backendORCinverisones.Infrastructure.Repositories;

public class NombreMarcaRepository : INombreMarcaRepository
{
    private readonly ApplicationDbContext _context;

    public NombreMarcaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<NombreMarca>> GetAllAsync()
    {
        return await _context.NombreMarcas
            .OrderBy(nm => nm.Nombre)
            .ToListAsync();
    }

    public async Task<NombreMarca?> GetByIdAsync(int id)
    {
        return await _context.NombreMarcas
            .FirstOrDefaultAsync(nm => nm.Id == id);
    }

    public async Task<NombreMarca> CreateAsync(NombreMarca nombreMarca)
    {
        nombreMarca.CreatedAt = DateTime.UtcNow;
        _context.NombreMarcas.Add(nombreMarca);
        await _context.SaveChangesAsync();
        return nombreMarca;
    }

    public async Task<NombreMarca> UpdateAsync(NombreMarca nombreMarca)
    {
        nombreMarca.UpdatedAt = DateTime.UtcNow;
        _context.NombreMarcas.Update(nombreMarca);
        await _context.SaveChangesAsync();
        return nombreMarca;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var nombreMarca = await GetByIdAsync(id);
        if (nombreMarca == null)
            return false;

        _context.NombreMarcas.Remove(nombreMarca);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsByNameAsync(string nombreMarca, int? excludeId = null)
    {
        var query = _context.NombreMarcas
            .Where(nm => nm.Nombre.ToLower() == nombreMarca.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(nm => nm.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
