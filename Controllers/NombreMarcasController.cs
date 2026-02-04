using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backendORCinverisones.Application.DTOs.NombreMarcas;
using backendORCinverisones.Application.Interfaces.Services;
using backendORCinverisones.Domain.Entities;
using backendORCinverisones.Infrastructure.Repositories;

namespace backendORCinverisones.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NombreMarcasController : ControllerBase
{
    private readonly INombreMarcaRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<NombreMarcasController> _logger;

    // Claves de caché
    private const string CACHE_KEY_ALL_MARCAS = "marcas:all";
    private const string CACHE_KEY_MARCA_BY_ID = "marcas:id:";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromHours(2); // Datos casi estáticos

    public NombreMarcasController(
        INombreMarcaRepository repository,
        ICacheService cacheService,
        ILogger<NombreMarcasController> logger)
    {
        _repository = repository;
        _cacheService = cacheService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var marcas = await _cacheService.GetOrCreateAsync(
            CACHE_KEY_ALL_MARCAS,
            async () =>
            {
                _logger.LogDebug("Cache miss para marcas, consultando BD...");
                var data = await _repository.GetAllAsync();
                return data.Select(nm => new NombreMarcaResponseDto
                {
                    Id = nm.Id,
                    Nombre = nm.Nombre,
                    IsActive = nm.IsActive,
                    CreatedAt = nm.CreatedAt,
                    UpdatedAt = nm.UpdatedAt
                }).ToList();
            },
            CACHE_DURATION);

        return Ok(marcas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var cacheKey = $"{CACHE_KEY_MARCA_BY_ID}{id}";
        
        var marca = await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                var data = await _repository.GetByIdAsync(id);
                if (data == null) return null;
                
                return new NombreMarcaResponseDto
                {
                    Id = data.Id,
                    Nombre = data.Nombre,
                    IsActive = data.IsActive,
                    CreatedAt = data.CreatedAt,
                    UpdatedAt = data.UpdatedAt
                };
            },
            CACHE_DURATION);

        if (marca == null)
            return NotFound(new { message = "Nombre de marca no encontrado" });

        return Ok(marca);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create([FromBody] CreateNombreMarcaRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _repository.ExistsByNameAsync(dto.Nombre))
            return BadRequest(new { message = "Ya existe un nombre de marca con ese nombre" });

        var nombreMarca = new NombreMarca
        {
            Nombre = dto.Nombre,
            IsActive = true
        };

        var created = await _repository.CreateAsync(nombreMarca);
        
        // Invalidar caché
        _cacheService.Remove(CACHE_KEY_ALL_MARCAS);

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
            return BadRequest(ModelState);

        var nombreMarca = await _repository.GetByIdAsync(id);
        if (nombreMarca == null)
            return NotFound(new { message = "Nombre de marca no encontrado" });

        if (await _repository.ExistsByNameAsync(dto.Nombre, id))
            return BadRequest(new { message = "Ya existe un nombre de marca con ese nombre" });

        nombreMarca.Nombre = dto.Nombre;
        nombreMarca.IsActive = dto.IsActive;

        var updated = await _repository.UpdateAsync(nombreMarca);
        
        // Invalidar caché
        _cacheService.Remove(CACHE_KEY_ALL_MARCAS);
        _cacheService.Remove($"{CACHE_KEY_MARCA_BY_ID}{id}");

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
            return NotFound(new { message = "Nombre de marca no encontrado" });

        // Invalidar caché
        _cacheService.Remove(CACHE_KEY_ALL_MARCAS);
        _cacheService.Remove($"{CACHE_KEY_MARCA_BY_ID}{id}");

        return Ok(new { message = "Nombre de marca eliminado exitosamente" });
    }

    [HttpDelete("delete-all")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteAll()
    {
        try
        {
            await _repository.DeleteAllAsync();
            _logger.LogInformation("TODAS las marcas eliminadas por {CurrentUser}", User.Identity?.Name);
            
            // Invalidar toda la caché de marcas
            _cacheService.RemoveByPrefix("marcas:");
            
            return Ok(new { message = "Todas las marcas han sido eliminadas correctamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al eliminar todas las marcas", error = ex.Message });
        }
    }
}
