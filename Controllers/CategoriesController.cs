using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backendORCinverisones.Application.DTOs.Categories;
using backendORCinverisones.Application.Interfaces.Repositories;
using backendORCinverisones.Domain.Entities;

namespace backendORCinverisones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryRepository categoryRepository, ILogger<CategoriesController> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryRepository.GetAllAsync();
        var response = categories.Select(c => new CategoryResponseDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        });

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category == null)
            return NotFound(new { message = "Categoría no encontrada" });

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

        return Ok(new { message = "Categoría eliminada correctamente" });
    }
}
