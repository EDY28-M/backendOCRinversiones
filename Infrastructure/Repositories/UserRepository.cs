using Microsoft.EntityFrameworkCore;
using backendORCinverisones.Application.Interfaces.Repositories;
using backendORCinverisones.Domain.Entities;
using backendORCinverisones.Infrastructure.Data;

namespace backendORCinverisones.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        // ✅ Usar compiled query pre-compilada (evita re-parsear SQL en cada login)
        return await CompiledQueries.GetUserByUsernameAsync(_context, username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        // ✅ Usar compiled query pre-compilada
        return await CompiledQueries.GetUserByEmailAsync(_context, email);
    }

    public async Task<User?> GetByIdWithRoleAsync(int id)
    {
        return await _dbSet
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public override async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _dbSet
            .Include(u => u.Role)
            .ToListAsync();
    }
}
