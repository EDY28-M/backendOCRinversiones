using Microsoft.AspNetCore.Mvc;
using backendORCinverisones.Application.DTOs.Auth;
using backendORCinverisones.Application.Interfaces.Services;

namespace backendORCinverisones.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _authService.LoginAsync(request);

        if (response == null)
            return Unauthorized(new { message = "Credenciales inválidas" });

        _logger.LogInformation("Usuario {Username} ha iniciado sesión", request.Username);
        return Ok(response);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _authService.ForgotPasswordAsync(request.Email);

        // Siempre retornar 200 por seguridad (no revelar si el email existe)
        return Ok(new { message = "Si el correo electrónico está registrado, recibirás un enlace para restablecer tu contraseña." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, message) = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);

        if (!success)
            return BadRequest(new { message });

        return Ok(new { message });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new { message = "Sesión cerrada correctamente" });
    }

    /// <summary>
    /// Endpoint ultraligero para despertar el servidor (warmup/keep-alive).
    /// No requiere autenticación. Respuesta mínima ~1ms.
    /// </summary>
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { status = "ok", timestamp = DateTime.UtcNow });
    }
}
