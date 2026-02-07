using System.ComponentModel.DataAnnotations;

namespace backendORCinverisones.Application.DTOs.Users;

public class UpdateUserRequestDto
{
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El username debe tener entre 3 y 50 caracteres")]
    public string? Username { get; set; }

    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string? Email { get; set; }

    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string? Password { get; set; }

    public int? RoleId { get; set; }
    public bool? IsActive { get; set; }
}
