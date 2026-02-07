using System.Security.Cryptography;
using System.Text;

namespace backendORCinverisones;

/// <summary>
/// Utilidad para generar claves JWT seguras
/// </summary>
public class JwtKeyGenerator
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Generador de Claves JWT ===\n");
        
        // Generar clave de 32 bytes (256 bits)
        var key32 = GenerateSecureKey(32);
        Console.WriteLine("Clave de 32 caracteres (256 bits):");
        Console.WriteLine(key32);
        Console.WriteLine();
        
        // Generar clave de 64 bytes (512 bits)
        var key64 = GenerateSecureKey(64);
        Console.WriteLine("Clave de 64 caracteres (512 bits):");
        Console.WriteLine(key64);
        Console.WriteLine();
        
        // Mostrar ejemplo de configuración
        Console.WriteLine("=== Configuración para appsettings.json ===\n");
        Console.WriteLine("\"Jwt\": {");
        Console.WriteLine($"  \"Key\": \"{key32}\",");
        Console.WriteLine("  \"Issuer\": \"ORCInversionesAPI\",");
        Console.WriteLine("  \"Audience\": \"ORCInversionesClient\",");
        Console.WriteLine("  \"ExpirationHours\": \"24\"");
        Console.WriteLine("}");
        Console.WriteLine();
        
        Console.WriteLine("Presiona cualquier tecla para salir...");
        Console.ReadKey();
    }
    
    private static string GenerateSecureKey(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=[]{}|;:,.<>?";
        var random = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(random);
        }
        
        var result = new StringBuilder(length);
        foreach (var b in random)
        {
            result.Append(chars[b % chars.Length]);
        }
        
        return result.ToString();
    }
}
