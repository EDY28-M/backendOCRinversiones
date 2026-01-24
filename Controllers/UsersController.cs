using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backendORCinverisones.Application.DTOs.Users;
using backendORCinverisones.Application.Interfaces.Repositories;
using backendORCinverisones.Application.Interfaces.Services;
using backendORCinverisones.Domain.Entities;

namespace backendORCinverisones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordService passwordService,
        ILogger<UsersController> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordService = passwordService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userRepository.GetAllAsync();
        var response = users.Select(u => new UserResponseDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt,
            RoleId = u.RoleId,
            RoleName = u.Role.Name
        });

        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userRepository.GetByIdWithRoleAsync(id);

        if (user == null)
            return NotFound(new { message = "Usuario no encontrado" });

        var response = new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            RoleId = user.RoleId,
            RoleName = user.Role.Name
        };

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _userRepository.ExistsAsync(u => u.Username == request.Username))
            return BadRequest(new { message = "El username ya existe" });

        if (await _userRepository.ExistsAsync(u => u.Email == request.Email))
            return BadRequest(new { message = "El email ya existe" });

        var role = await _roleRepository.GetByIdAsync(request.RoleId);
        if (role == null)
            return BadRequest(new { message = "El rol especificado no existe" });

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordService.HashPassword(request.Password),
            RoleId = request.RoleId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userRepository.AddAsync(user);
        _logger.LogInformation("Usuario {Username} creado por {CurrentUser}", user.Username, User.Identity?.Name);

        var createdUser = await _userRepository.GetByIdWithRoleAsync(user.Id);
        var response = new UserResponseDto
        {
            Id = createdUser!.Id,
            Username = createdUser.Username,
            Email = createdUser.Email,
            IsActive = createdUser.IsActive,
            CreatedAt = createdUser.CreatedAt,
            UpdatedAt = createdUser.UpdatedAt,
            RoleId = createdUser.RoleId,
            RoleName = createdUser.Role.Name
        };

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return NotFound(new { message = "Usuario no encontrado" });

        if (request.Username != null)
        {
            if (await _userRepository.ExistsAsync(u => u.Username == request.Username && u.Id != id))
                return BadRequest(new { message = "El username ya existe" });
            user.Username = request.Username;
        }

        if (request.Email != null)
        {
            if (await _userRepository.ExistsAsync(u => u.Email == request.Email && u.Id != id))
                return BadRequest(new { message = "El email ya existe" });
            user.Email = request.Email;
        }

        if (request.Password != null)
            user.PasswordHash = _passwordService.HashPassword(request.Password);

        if (request.RoleId.HasValue)
        {
            var role = await _roleRepository.GetByIdAsync(request.RoleId.Value);
            if (role == null)
                return BadRequest(new { message = "El rol especificado no existe" });
            user.RoleId = request.RoleId.Value;
        }

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        _logger.LogInformation("Usuario {Username} actualizado por {CurrentUser}", user.Username, User.Identity?.Name);

        var updatedUser = await _userRepository.GetByIdWithRoleAsync(id);
        var response = new UserResponseDto
        {
            Id = updatedUser!.Id,
            Username = updatedUser.Username,
            Email = updatedUser.Email,
            IsActive = updatedUser.IsActive,
            CreatedAt = updatedUser.CreatedAt,
            UpdatedAt = updatedUser.UpdatedAt,
            RoleId = updatedUser.RoleId,
            RoleName = updatedUser.Role.Name
        };

        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return NotFound(new { message = "Usuario no encontrado" });

        await _userRepository.DeleteAsync(user);
        _logger.LogInformation("Usuario {Username} eliminado por {CurrentUser}", user.Username, User.Identity?.Name);

        return Ok(new { message = "Usuario eliminado correctamente" });
    }
}
