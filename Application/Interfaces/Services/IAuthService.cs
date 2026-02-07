using backendORCinverisones.Application.DTOs.Auth;

namespace backendORCinverisones.Application.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    string GenerateJwtToken(int userId, string username, string roleName);
}
