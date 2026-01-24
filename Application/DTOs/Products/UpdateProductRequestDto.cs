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

    [StringLength(500, ErrorMessage = "La URL de imagen no puede exceder 500 caracteres")]
    public string? ImagenPrincipal { get; set; }

    [StringLength(500, ErrorMessage = "La URL de imagen no puede exceder 500 caracteres")]
    public string? Imagen2 { get; set; }

    [StringLength(500, ErrorMessage = "La URL de imagen no puede exceder 500 caracteres")]
    public string? Imagen3 { get; set; }

    [StringLength(500, ErrorMessage = "La URL de imagen no puede exceder 500 caracteres")]
    public string? Imagen4 { get; set; }

    [StringLength(5000, ErrorMessage = "La descripción no puede exceder 5000 caracteres")]
    public string? Descripcion { get; set; }

    [StringLength(10000, ErrorMessage = "La ficha técnica no puede exceder 10000 caracteres")]
    public string? FichaTecnica { get; set; }

    public int? CategoryId { get; set; }
    public int? MarcaId { get; set; }
    public bool? IsActive { get; set; }
}
