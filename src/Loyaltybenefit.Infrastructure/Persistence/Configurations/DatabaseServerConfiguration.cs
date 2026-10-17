using Loyaltybenefit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loyaltybenefit.Infrastructure.Persistence.Configurations;

public sealed class DatabaseServerConfiguration
    : IEntityTypeConfiguration<DatabaseServer>
{
    public void Configure(
        EntityTypeBuilder<DatabaseServer> builder)
    {
        builder.ToTable("DatabaseServers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.ProviderCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.EngineCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Host)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Port)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();
    }
}
