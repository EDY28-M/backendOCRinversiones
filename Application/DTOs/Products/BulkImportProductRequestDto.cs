using System.ComponentModel.DataAnnotations;

namespace backendORCinverisones.Application.DTOs.Products;

public class BulkImportProductRequestDto
{
    [Required(ErrorMessage = "La lista de productos es requerida")]
    public List<BulkProductItemDto> Products { get; set; } = new();
    
    /// <summary>
    /// Si es true, crea automáticamente marcas y categorías que no existen
    /// </summary>
    public bool AutoCreateEntities { get; set; } = true;
}

public class BulkProductItemDto
{
    [Required(ErrorMessage = "El código es requerido")]
    [StringLength(100, ErrorMessage = "El código no puede exceder 100 caracteres")]
    public string Codigo { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El código comercial no puede exceder 100 caracteres")]
    public string? CodigoComer { get; set; }

    [Required(ErrorMessage = "El nombre del producto es requerido")]
    [StringLength(300, ErrorMessage = "El nombre del producto no puede exceder 300 caracteres")]
    public string Producto { get; set; } = string.Empty;

    [StringLength(5000, ErrorMessage = "La descripción no puede exceder 5000 caracteres")]
    public string? Descripcion { get; set; }

    [StringLength(10000, ErrorMessage = "La ficha técnica no puede exceder 10000 caracteres")]
    public string? FichaTecnica { get; set; }

    // Ahora acepta nombre de marca en lugar de ID
    public int? MarcaId { get; set; }
    public string? MarcaNombre { get; set; }
    
    // Ahora acepta nombre de categoría en lugar de ID
    public int? CategoryId { get; set; }
    public string? CategoriaNombre { get; set; }

    [Url(ErrorMessage = "La URL de la imagen principal no es válida")]
    public string? ImagenPrincipal { get; set; }

    [Url(ErrorMessage = "La URL de la imagen 2 no es válida")]
    public string? Imagen2 { get; set; }

    [Url(ErrorMessage = "La URL de la imagen 3 no es válida")]
    public string? Imagen3 { get; set; }

    [Url(ErrorMessage = "La URL de la imagen 4 no es válida")]
    public string? Imagen4 { get; set; }
}

public class BulkImportResultDto
{
    public int Imported { get; set; }
    public int Failed { get; set; }
    public int Duplicates { get; set; }
    public int MarcasCreated { get; set; }
    public int CategoriasCreated { get; set; }
    public List<string> Errors { get; set; } = new();
}
