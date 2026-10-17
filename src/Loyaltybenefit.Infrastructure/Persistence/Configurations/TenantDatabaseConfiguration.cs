using Loyaltybenefit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loyaltybenefit.Infrastructure.Persistence.Configurations;

public sealed class TenantDatabaseConfiguration
    : IEntityTypeConfiguration<TenantDatabase>
{
    public void Configure(
        EntityTypeBuilder<TenantDatabase> builder)
    {
        builder.ToTable("TenantDatabases");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LogicalName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.PhysicalDatabaseName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.DatabaseServerId)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(x => x.TenantId);

        builder.HasIndex(x => x.DatabaseServerId);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<DatabaseServer>()
            .WithMany()
            .HasForeignKey(x => x.DatabaseServerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
