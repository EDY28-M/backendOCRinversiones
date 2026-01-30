using System.ComponentModel.DataAnnotations;

namespace backendORCinverisones.Application.DTOs.Products;

public class CreateProductRequestDto
{
    [Required(ErrorMessage = "El código es requerido")]
    [StringLength(100, ErrorMessage = "El código no puede exceder 100 caracteres")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código comercial es requerido")]
    [StringLength(100, ErrorMessage = "El código comercial no puede exceder 100 caracteres")]
    public string CodigoComer { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del producto es requerido")]
    [StringLength(300, ErrorMessage = "El nombre del producto no puede exceder 300 caracteres")]
    public string Producto { get; set; } = string.Empty;

    // Imágenes en Base64 o URLs - sin límite de longitud
    public string? ImagenPrincipal { get; set; }
    public string? Imagen2 { get; set; }
    public string? Imagen3 { get; set; }
    public string? Imagen4 { get; set; }

    [StringLength(5000, ErrorMessage = "La descripción no puede exceder 5000 caracteres")]
    public string? Descripcion { get; set; }

    [StringLength(10000, ErrorMessage = "La ficha técnica no puede exceder 10000 caracteres")]
    public string? FichaTecnica { get; set; }

    // Destacado en home (opcional)
    public bool IsFeatured { get; set; } = false;

    [Required(ErrorMessage = "La categoría es requerida")]
    public int CategoryId { get; set; }
    
    [Required(ErrorMessage = "La marca es requerida")]
    public int MarcaId { get; set; }
}
