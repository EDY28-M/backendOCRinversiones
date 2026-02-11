namespace backendORCinverisones.Application.DTOs.Products;

/// <summary>
/// DTO liviano para el historial de productos recientes.
/// Solo campos esenciales para la vista de resumen.
/// </summary>
public class RecentProductDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Producto { get; set; } = string.Empty;
    public string MarcaNombre { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
