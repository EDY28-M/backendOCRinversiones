using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backendORCinverisones.Domain.Entities;

namespace backendORCinverisones.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Description)
            .HasMaxLength(200);

        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        // Índices
        builder.HasIndex(r => r.Name)
            .IsUnique();

        // Seed data
        builder.HasData(
            new Role { Id = 1, Name = "Administrador", Description = "Acceso total al sistema", CreatedAt = DateTime.Now },
            new Role { Id = 2, Name = "Vendedor", Description = "Acceso restringido a productos", CreatedAt = DateTime.Now }
        );
    }
}
