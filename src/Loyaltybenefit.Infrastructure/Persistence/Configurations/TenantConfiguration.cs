using Loyaltybenefit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loyaltybenefit.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration
    : IEntityTypeConfiguration<Tenant>
{
    public void Configure(
        EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.IndustryId)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        // MAPEO DE LAS NUEVAS COLUMNAS DE BRANDING
        builder.Property(x => x.LogoUrl)
            .HasMaxLength(255)
            .IsRequired(false); // Permite nulos

        builder.Property(x => x.PrimaryColorHex)
            .HasMaxLength(7)
            .IsRequired(false);

        builder.Property(x => x.SecondaryColorHex)
            .HasMaxLength(7)
            .IsRequired(false);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.HasOne<Industry>()
            .WithMany()
            .HasForeignKey(x => x.IndustryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
