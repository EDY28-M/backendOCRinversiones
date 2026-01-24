using backendORCinverisones.Domain.Entities;
using backendORCinverisones.Infrastructure.Data;

namespace backendORCinverisones.Infrastructure;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Verificar si ya existen usuarios
        if (context.Users.Any())
            return;

        // Crear usuario administrador por defecto
        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@orcinversiones.com",
            // Contraseña: Admin123
            PasswordHash = "$2a$11$xJKVqYXN5YHJfGJdKk5h5.N1xKzO9QqQX8Z3rQK5sX6Z8K9vQr5YW",
            RoleId = 1, // Administrador
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
    }
}
