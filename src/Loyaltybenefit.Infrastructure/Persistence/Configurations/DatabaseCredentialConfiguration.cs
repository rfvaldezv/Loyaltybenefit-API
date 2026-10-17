using Loyaltybenefit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loyaltybenefit.Infrastructure.Persistence.Configurations;

public sealed class DatabaseCredentialConfiguration
    : IEntityTypeConfiguration<DatabaseCredential>
{
    public void Configure(
        EntityTypeBuilder<DatabaseCredential> builder)
    {
        builder.ToTable("DatabaseCredentials");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Username)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.PasswordSecret)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasOne<DatabaseServer>()
            .WithMany()
            .HasForeignKey(x => x.DatabaseServerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
