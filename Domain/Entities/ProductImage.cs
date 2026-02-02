namespace backendORCinverisones.Domain.Entities;

public class ProductImage
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int OrderIndex { get; set; } // 0 = Principal
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relación
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
