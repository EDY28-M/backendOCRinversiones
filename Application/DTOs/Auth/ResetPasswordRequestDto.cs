using System.ComponentModel.DataAnnotations;

namespace backendORCinverisones.Application.DTOs.Auth;

public class ResetPasswordRequestDto
{
    [Required(ErrorMessage = "El token es requerido")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es requerida")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string NewPassword { get; set; } = string.Empty;
}
