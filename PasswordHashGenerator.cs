using BCrypt.Net;

namespace backendORCinverisones;

/// <summary>
/// Generador de hashes BCrypt para contraseñas
/// Ejecutar: dotnet run --project . PasswordHashGenerator.cs
/// </summary>
public class PasswordHashGenerator
{
    public static void GenerateHashes()
    {
        Console.WriteLine("=== GENERADOR DE HASHES BCRYPT ===\n");
        
        // Contraseñas comunes de prueba
        var passwords = new[]
        {
            "Admin123!",
            "Admin123",
            "Vendedor123!",
            "password123"
        };
        
        foreach (var password in passwords)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            Console.WriteLine($"Contraseña: {password}");
            Console.WriteLine($"Hash:       {hash}");
            Console.WriteLine($"Longitud:   {hash.Length} caracteres");
            Console.WriteLine();
        }
        
        Console.WriteLine("=== SQL PARA INSERTAR USUARIO ADMIN ===\n");
        var adminHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");
        
        Console.WriteLine(@$"
USE ORCInversiones;

-- Eliminar usuario admin si existe
DELETE FROM Users WHERE Username = 'admin';

-- Insertar usuario admin con hash correcto
INSERT INTO Users (Username, Email, PasswordHash, RoleId, IsActive, CreatedAt)
VALUES (
    'admin',
    'admin@orcinversiones.com',
    '{adminHash}',
    1,
    1,
    GETDATE()
);

-- Verificar
SELECT u.Id, u.Username, u.Email, r.Name as Role, LEN(u.PasswordHash) as HashLength
FROM Users u
JOIN Roles r ON u.RoleId = r.Id;
");
        
        Console.WriteLine("\n=== COPIAR Y EJECUTAR EN SQL SERVER ===");
    }
}
