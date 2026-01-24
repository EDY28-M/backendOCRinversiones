using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backendORCinverisones.Application.DTOs.Roles;
using backendORCinverisones.Application.Interfaces.Repositories;
using backendORCinverisones.Domain.Entities;

namespace backendORCinverisones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class RolesController : ControllerBase
{
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IRoleRepository roleRepository, ILogger<RolesController> logger)
    {
        _roleRepository = roleRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _roleRepository.GetAllAsync();
        var response = roles.Select(r => new RoleResponseDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            CreatedAt = r.CreatedAt
        });

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var role = await _roleRepository.GetByIdAsync(id);

        if (role == null)
            return NotFound(new { message = "Rol no encontrado" });

        var response = new RoleResponseDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt
        };

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _roleRepository.ExistsAsync(r => r.Name == request.Name))
            return BadRequest(new { message = "El rol ya existe" });

        var role = new Role
        {
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        await _roleRepository.AddAsync(role);
        _logger.LogInformation("Rol {RoleName} creado por {CurrentUser}", role.Name, User.Identity?.Name);

        var response = new RoleResponseDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = role.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
            return NotFound(new { message = "Rol no encontrado" });

        if (request.Name != null)
        {
            if (await _roleRepository.ExistsAsync(r => r.Name == request.Name && r.Id != id))
                return BadRequest(new { message = "El nombre del rol ya existe" });
            role.Name = request.Name;
        }

        if (request.Description != null)
            role.Description = request.Description;

        await _roleRepository.UpdateAsync(role);
        _logger.LogInformation("Rol {RoleName} actualizado por {CurrentUser}", role.Name, User.Identity?.Name);

        var response = new RoleResponseDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt
        };

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
            return NotFound(new { message = "Rol no encontrado" });

        if (id == 1 || id == 2)
            return BadRequest(new { message = "No se pueden eliminar los roles predeterminados del sistema" });

        await _roleRepository.DeleteAsync(role);
        _logger.LogInformation("Rol {RoleName} eliminado por {CurrentUser}", role.Name, User.Identity?.Name);

        return Ok(new { message = "Rol eliminado correctamente" });
    }
}
