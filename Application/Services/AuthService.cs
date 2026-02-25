using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using backendORCinverisones.Application.DTOs.Auth;
using backendORCinverisones.Application.Interfaces.Repositories;
using backendORCinverisones.Application.Interfaces.Services;

namespace backendORCinverisones.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository, 
        IPasswordService passwordService,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        if (user == null || !user.IsActive)
            return null;

        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            return null;

        var token = GenerateJwtToken(user.Id, user.Username, user.Role.Name);
        var expiresAt = DateTime.UtcNow.AddHours(
            double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24"));

        return new LoginResponseDto
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.Name,
            ExpiresAt = expiresAt
        };
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("🔒 Solicitud de reset para email no existente: {Email}", email);
            // Retornar true por seguridad (no revelar si el email existe)
            return true;
        }

        // Generar token seguro
        var token = GenerateSecureToken();
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _userRepository.UpdateAsync(user);

        // Construir URL de reset
        var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
        var resetLink = $"{frontendUrl}/admin/reset-password?token={token}";

        _logger.LogInformation("🔑 Token de reset generado para: {Email}", email);

        // Enviar email via Brevo con plantilla #7 (Recuperación de contraseña)
        var templateId = int.Parse(_configuration["EmailSettings:BrevoTemplateId"] ?? "7");
        var templateParams = new Dictionary<string, object>
        {
            ["name"] = user.Username,
            ["email"] = user.Email,
            ["resetLink"] = resetLink,
            ["senderName"] = _configuration["EmailSettings:SenderName"] ?? "ORC Inversiones Perú"
        };

        var sent = await _emailService.SendTemplateEmailAsync(
            toEmail: user.Email,
            toName: user.Username,
            templateId: templateId,
            templateParams: templateParams);

        if (!sent)
        {
            _logger.LogError("❌ No se pudo enviar email de recuperación a: {Email}", email);
            // Igual retornar true por seguridad
        }

        return true;
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(string token, string newPassword)
    {
        var user = await _userRepository.GetByResetTokenAsync(token);

        if (user == null)
        {
            _logger.LogWarning("🔒 Intento de reset con token inválido");
            return (false, "El enlace de recuperación no es válido o ya fue utilizado");
        }

        if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            _logger.LogWarning("🔒 Token expirado para usuario: {Username}", user.Username);
            // Limpiar token expirado
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await _userRepository.UpdateAsync(user);
            return (false, "El enlace de recuperación ha expirado. Solicita uno nuevo");
        }

        // Actualizar contraseña
        user.PasswordHash = _passwordService.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("✅ Contraseña actualizada para: {Username}", user.Username);
        return (true, "Contraseña actualizada correctamente");
    }

    public string GenerateJwtToken(int userId, string username, string roleName)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(ClaimTypes.Role, roleName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}
