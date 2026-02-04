using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backendORCinverisones.Application.DTOs.Categories;
using backendORCinverisones.Application.Interfaces.Repositories;
using backendORCinverisones.Application.Interfaces.Services;
using backendORCinverisones.Application.Logging;
using backendORCinverisones.Domain.Entities;

namespace backendORCinverisones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CategoriesController> _logger;

    // Claves de caché
    private const string CACHE_KEY_ALL_CATEGORIES = "categories:all";
    private const string CACHE_KEY_CATEGORY_BY_ID = "categories:id:";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromHours(2); // Datos casi estáticos

    public CategoriesController(
        ICategoryRepository categoryRepository, 
        ICacheService cacheService,
        ILogger<CategoriesController> logger)
    {
        _categoryRepository = categoryRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _cacheService.GetOrCreateAsync(
            CACHE_KEY_ALL_CATEGORIES,
            async () =>
            {
                _logger.LogDebug("Cache miss para categorías, consultando BD...");
                var data = await _categoryRepository.GetAllAsync();
                return data.Select(c => new CategoryResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToList();
            },
            CACHE_DURATION);

        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var cacheKey = $"{CACHE_KEY_CATEGORY_BY_ID}{id}";
        
        var category = await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                var data = await _categoryRepository.GetByIdAsync(id);
                if (data == null) return null;
                
                return new CategoryResponseDto
                {
                    Id = data.Id,
                    Name = data.Name,
                    Description = data.Description,
                    IsActive = data.IsActive,
                    CreatedAt = data.CreatedAt,
                    UpdatedAt = data.UpdatedAt
                };
            },
            CACHE_DURATION);

        if (category == null)
            return NotFound(new { message = "Categoría no encontrada" });

        return Ok(category);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _categoryRepository.ExistsAsync(c => c.Name == request.Name))
            return BadRequest(new { message = "La categoría ya existe" });

        var category = new Category
        {
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _categoryRepository.AddAsync(category);
        _logger.LogInformation("Categoría {CategoryName} creada por {CurrentUser}", category.Name, User.Identity?.Name);

        // Invalidar caché
        _cacheService.Remove(CACHE_KEY_ALL_CATEGORIES);

        var response = new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return NotFound(new { message = "Categoría no encontrada" });

        if (request.Name != null)
        {
            if (await _categoryRepository.ExistsAsync(c => c.Name == request.Name && c.Id != id))
                return BadRequest(new { message = "El nombre de la categoría ya existe" });
            category.Name = request.Name;
        }

        if (request.Description != null)
            category.Description = request.Description;

        if (request.IsActive.HasValue)
            category.IsActive = request.IsActive.Value;

        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepository.UpdateAsync(category);
        _logger.LogInformation("Categoría {CategoryName} actualizada por {CurrentUser}", category.Name, User.Identity?.Name);

        // Invalidar caché
        _cacheService.Remove(CACHE_KEY_ALL_CATEGORIES);
        _cacheService.Remove($"{CACHE_KEY_CATEGORY_BY_ID}{id}");

        var response = new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };

        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return NotFound(new { message = "Categoría no encontrada" });

        await _categoryRepository.DeleteAsync(category);
        _logger.LogInformation("Categoría {CategoryName} eliminada por {CurrentUser}", category.Name, User.Identity?.Name);

        // Invalidar caché
        _cacheService.Remove(CACHE_KEY_ALL_CATEGORIES);
        _cacheService.Remove($"{CACHE_KEY_CATEGORY_BY_ID}{id}");

        return Ok(new { message = "Categoría eliminada correctamente" });
    }

    [HttpDelete("delete-all")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteAll()
    {
        try
        {
            await _categoryRepository.DeleteAllAsync();
            _logger.LogInformation("TODAS las categorías eliminadas por {CurrentUser}", User.Identity?.Name);
            
            // Invalidar toda la caché de categorías
            _cacheService.RemoveByPrefix("categories:");
            
            return Ok(new { message = "Todas las categorías han sido eliminadas correctamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al eliminar todas las categorías", error = ex.Message });
        }
    }
}
