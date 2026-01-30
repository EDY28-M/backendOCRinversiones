using backendORCinverisones.Application.DTOs.NombreMarcas;
using backendORCinverisones.Domain.Entities;
using backendORCinverisones.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backendORCinverisones.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NombreMarcasController : ControllerBase
{
    private readonly INombreMarcaRepository _repository;
    private readonly ILogger<NombreMarcasController> _logger;

    public NombreMarcasController(
        INombreMarcaRepository repository,
        ILogger<NombreMarcasController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var nombreMarcas = await _repository.GetAllAsync();
        var response = nombreMarcas.Select(nm => new NombreMarcaResponseDto
        {
            Id = nm.Id,
            Nombre = nm.Nombre,
            IsActive = nm.IsActive,
            CreatedAt = nm.CreatedAt,
            UpdatedAt = nm.UpdatedAt
        });

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var nombreMarca = await _repository.GetByIdAsync(id);
        if (nombreMarca == null)
        {
            return NotFound(new { message = "Nombre de marca no encontrado" });
        }

        var response = new NombreMarcaResponseDto
        {
            Id = nombreMarca.Id,
            Nombre = nombreMarca.Nombre,
            IsActive = nombreMarca.IsActive,
            CreatedAt = nombreMarca.CreatedAt,
            UpdatedAt = nombreMarca.UpdatedAt
        };

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create([FromBody] CreateNombreMarcaRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validar que no exista el nombre
        if (await _repository.ExistsByNameAsync(dto.Nombre))
        {
            return BadRequest(new { message = "Ya existe un nombre de marca con ese nombre" });
        }

        var nombreMarca = new NombreMarca
        {
            Nombre = dto.Nombre,
            IsActive = true
        };

        var created = await _repository.CreateAsync(nombreMarca);

        var response = new NombreMarcaResponseDto
        {
            Id = created.Id,
            Nombre = created.Nombre,
            IsActive = created.IsActive,
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateNombreMarcaRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var nombreMarca = await _repository.GetByIdAsync(id);
        if (nombreMarca == null)
        {
            return NotFound(new { message = "Nombre de marca no encontrado" });
        }

        // Validar que no exista el nombre (excluyendo el actual)
        if (await _repository.ExistsByNameAsync(dto.Nombre, id))
        {
            return BadRequest(new { message = "Ya existe un nombre de marca con ese nombre" });
        }

        nombreMarca.Nombre = dto.Nombre;
        nombreMarca.IsActive = dto.IsActive;

        var updated = await _repository.UpdateAsync(nombreMarca);

        var response = new NombreMarcaResponseDto
        {
            Id = updated.Id,
            Nombre = updated.Nombre,
            IsActive = updated.IsActive,
            CreatedAt = updated.CreatedAt,
            UpdatedAt = updated.UpdatedAt
        };

        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _repository.DeleteAsync(id);
        if (!result)
        {
            return NotFound(new { message = "Nombre de marca no encontrado" });
        }

        return Ok(new { message = "Nombre de marca eliminado exitosamente" });
    }
}
