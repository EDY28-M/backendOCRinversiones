using backendORCinverisones.Application.Interfaces.Services;

namespace backendORCinverisones.Application.Services;

public class PasswordService : IPasswordService
{
    // Work factor 10 = ~100ms por verificación (seguro y rápido)
    // Work factor 11 (default) = ~200-400ms (innecesariamente lento para esta app)
    private const int WorkFactor = 10;

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
