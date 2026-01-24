using System.ComponentModel.DataAnnotations;

namespace backendORCinverisones.Application.DTOs.NombreMarcas;

public class CreateNombreMarcaRequestDto
{
    [Required(ErrorMessage = "El nombre de marca es requerido")]
    [StringLength(200, ErrorMessage = "El nombre de marca no puede exceder 200 caracteres")]
    public string Nombre { get; set; } = string.Empty;
}
