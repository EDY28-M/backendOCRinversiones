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
            new Role { Id = 1, Name = "Administrador", Description = "Acceso total al sistema", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = 2, Name = "Vendedor", Description = "Acceso restringido a productos", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
