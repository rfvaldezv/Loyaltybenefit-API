using Loyaltybenefit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loyaltybenefit.Infrastructure.Persistence.Configurations;

public sealed class HotelConfiguration
    : IEntityTypeConfiguration<Hotel>
{
    public void Configure(
        EntityTypeBuilder<Hotel> builder)
    {
        builder.ToTable("Hotels");

        builder.HasKey(x => x.Id);

        // --------------------------------------------------------
        // Organization
        // --------------------------------------------------------

        builder.Property(x => x.OrganizationId)
            .IsRequired();

        builder.HasIndex(x => x.OrganizationId);

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // --------------------------------------------------------
        // Basic information
        // --------------------------------------------------------

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.City)
            .HasMaxLength(100);

        builder.Property(x => x.State)
            .HasMaxLength(100);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();


        // --------------------------------------------------------
        // Unique hotel code within an organization
        // --------------------------------------------------------

        builder.HasIndex(
            x => new
            {
                x.OrganizationId,
                x.Code
            })
            .IsUnique();
    }
}
