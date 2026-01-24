namespace backendORCinverisones.Application.DTOs.Products;

public class ProductResponseDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string CodigoComer { get; set; } = string.Empty;
    public string Producto { get; set; } = string.Empty;
    public string? ImagenPrincipal { get; set; }
    public string? Imagen2 { get; set; }
    public string? Imagen3 { get; set; }
    public string? Imagen4 { get; set; }
    public string? Descripcion { get; set; }
    public string? FichaTecnica { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int MarcaId { get; set; }
    public string MarcaNombre { get; set; } = string.Empty;
}
