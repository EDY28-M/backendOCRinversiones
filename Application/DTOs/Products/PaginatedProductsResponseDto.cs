namespace backendORCinverisones.Application.DTOs.Products;

/// <summary>
/// Respuesta paginada genérica para listados de productos
/// </summary>
public class PaginatedProductsResponseDto
{
    public List<ProductResponseDto> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
}
