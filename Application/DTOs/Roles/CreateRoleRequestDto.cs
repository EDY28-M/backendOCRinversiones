using System.ComponentModel.DataAnnotations;

namespace backendORCinverisones.Application.DTOs.Roles;

public class CreateRoleRequestDto
{
    [Required(ErrorMessage = "El nombre del rol es requerido")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
    public string Description { get; set; } = string.Empty;
}
