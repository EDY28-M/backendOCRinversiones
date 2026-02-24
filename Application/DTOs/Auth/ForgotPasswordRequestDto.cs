using System.ComponentModel.DataAnnotations;

namespace backendORCinverisones.Application.DTOs.Auth;

public class ForgotPasswordRequestDto
{
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string Email { get; set; } = string.Empty;
}
