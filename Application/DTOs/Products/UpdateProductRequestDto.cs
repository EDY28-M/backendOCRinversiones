using System.ComponentModel.DataAnnotations;

namespace backendORCinverisones.Application.DTOs.Products;

public class UpdateProductRequestDto
{
    [StringLength(100, ErrorMessage = "El código no puede exceder 100 caracteres")]
    public string? Codigo { get; set; }

    [StringLength(100, ErrorMessage = "El código comercial no puede exceder 100 caracteres")]
    public string? CodigoComer { get; set; }

    [StringLength(300, ErrorMessage = "El nombre del producto no puede exceder 300 caracteres")]
    public string? Producto { get; set; }

    // Imágenes en Base64 o URLs - sin límite de longitud
    public string? ImagenPrincipal { get; set; }
    public string? Imagen2 { get; set; }
    public string? Imagen3 { get; set; }
    public string? Imagen4 { get; set; }

    [StringLength(5000, ErrorMessage = "La descripción no puede exceder 5000 caracteres")]
    public string? Descripcion { get; set; }

    [StringLength(10000, ErrorMessage = "La ficha técnica no puede exceder 10000 caracteres")]
    public string? FichaTecnica { get; set; }

    public int? CategoryId { get; set; }
    public int? MarcaId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsFeatured { get; set; }
}
