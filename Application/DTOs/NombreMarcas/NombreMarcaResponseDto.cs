namespace backendORCinverisones.Application.DTOs.NombreMarcas;

public class NombreMarcaResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
