using backendORCinverisones.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backendORCinverisones.Infrastructure.Data.Configurations;

public class NombreMarcaConfiguration : IEntityTypeConfiguration<NombreMarca>
{
    public void Configure(EntityTypeBuilder<NombreMarca> builder)
    {
        builder.ToTable("NombreMarcas");

        builder.HasKey(nm => nm.Id);

        builder.Property(nm => nm.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(nm => nm.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(nm => nm.CreatedAt)
            .IsRequired();

        builder.Property(nm => nm.UpdatedAt)
            .IsRequired(false);

        builder.HasIndex(nm => nm.Nombre)
            .IsUnique();
    }
}
