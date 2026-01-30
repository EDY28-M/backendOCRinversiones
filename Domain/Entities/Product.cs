namespace backendORCinverisones.Domain.Entities;

public class Product
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
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    public int MarcaId { get; set; }
    public NombreMarca Marca { get; set; } = null!;
}
