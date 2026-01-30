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

        builder.Property(p => p.IsFeatured)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.Property(p => p.UpdatedAt)
            .IsRequired(false);

        // ✅ ÍNDICES OPTIMIZADOS para queries comunes

        // Índices únicos para códigos (búsquedas exactas)
        builder.HasIndex(p => p.Codigo).IsUnique();
        builder.HasIndex(p => p.CodigoComer).IsUnique();

        // ✅ Índice compuesto para filtros por categoría activa (GetAvailableProductsPagedAsync)
        builder.HasIndex(p => new { p.IsActive, p.CategoryId })
            .HasDatabaseName("IX_Products_IsActive_CategoryId");

        // ✅ Índice compuesto para filtros por marca activa (GetPublicActiveProductsPagedAsync)
        builder.HasIndex(p => new { p.IsActive, p.MarcaId })
            .HasDatabaseName("IX_Products_IsActive_MarcaId");

        // ✅ Índice para ordenamiento por fecha (usado en paginación)
        builder.HasIndex(p => p.CreatedAt)
            .HasDatabaseName("IX_Products_CreatedAt");

        // ✅ Índice compuesto para búsquedas públicas (IsActive + CreatedAt)
        builder.HasIndex(p => new { p.IsActive, p.CreatedAt })
            .HasDatabaseName("IX_Products_IsActive_CreatedAt");

        // ✅ Índice compuesto para destacados (IsFeatured + IsActive + CreatedAt)
        builder.HasIndex(p => new { p.IsFeatured, p.IsActive, p.CreatedAt })
            .HasDatabaseName("IX_Products_IsFeatured_IsActive_CreatedAt");

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
