using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backendORCinverisones.Domain.Entities;

namespace backendORCinverisones.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Codigo)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.CodigoComer)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Producto)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(p => p.ImagenPrincipal)
            .HasColumnType("NVARCHAR(MAX)");

        builder.Property(p => p.Imagen2)
            .HasColumnType("NVARCHAR(MAX)");

        builder.Property(p => p.Imagen3)
            .HasColumnType("NVARCHAR(MAX)");

        builder.Property(p => p.Imagen4)
            .HasColumnType("NVARCHAR(MAX)");

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.Property(p => p.UpdatedAt)
            .IsRequired(false);

        // Índices - Codigo es el principal
        builder.HasIndex(p => p.Codigo);
        builder.HasIndex(p => p.CodigoComer);

        // Relaciones
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(p => p.Marca)
            .WithMany(m => m.Products)
            .HasForeignKey(p => p.MarcaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
