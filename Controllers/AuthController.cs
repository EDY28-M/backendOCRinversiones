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

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new { message = "Sesión cerrada correctamente" });
    }
}
