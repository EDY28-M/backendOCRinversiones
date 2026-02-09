using System.ComponentModel.DataAnnotations;

namespace backendORCinverisones.Application.DTOs.Contact;

public class ContactRequestDto
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string Email { get; set; } = string.Empty;

    [StringLength(30, ErrorMessage = "El teléfono no puede exceder 30 caracteres")]
    public string? Phone { get; set; }

    [StringLength(120, ErrorMessage = "El asunto no puede exceder 120 caracteres")]
    public string? Subject { get; set; }

    [Required(ErrorMessage = "El mensaje es requerido")]
    [StringLength(5000, MinimumLength = 10, ErrorMessage = "El mensaje debe tener entre 10 y 5000 caracteres")]
    public string Message { get; set; } = string.Empty;
}
