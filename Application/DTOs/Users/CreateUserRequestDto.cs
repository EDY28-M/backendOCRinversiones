using System.ComponentModel.DataAnnotations;

namespace backendORCinverisones.Application.DTOs.Users;

public class CreateUserRequestDto
{
    [Required(ErrorMessage = "El username es requerido")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El username debe tener entre 3 y 50 caracteres")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [StringLength(100, MinimumLength = 12, ErrorMessage = "La contraseña debe tener al menos 12 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{12,}$",
        ErrorMessage = "La contraseña debe contener al menos: 1 mayúscula, 1 minúscula, 1 número y 1 carácter especial (@$!%*?&#)")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es requerido")]
    public int RoleId { get; set; }
}
