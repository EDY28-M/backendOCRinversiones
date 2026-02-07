namespace backendORCinverisones.Domain.Entities;

public class NombreMarca
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
