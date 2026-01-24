using System.ComponentModel.DataAnnotations;

namespace backendORCinverisones.Application.DTOs.Categories;

public class UpdateCategoryRequestDto
{
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    public string? Name { get; set; }

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }
}
