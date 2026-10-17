using Loyaltybenefit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loyaltybenefit.Infrastructure.Persistence.Configurations;

public sealed class RedemptionConfiguration
    : IEntityTypeConfiguration<Redemption>
{
    public void Configure(
        EntityTypeBuilder<Redemption> builder)
    {
        builder.ToTable("Redemptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LoyaltyAccountId)
            .IsRequired();

        builder.HasIndex(x => x.LoyaltyAccountId);

        builder.HasOne<LoyaltyAccount>()
            .WithMany()
            .HasForeignKey(x => x.LoyaltyAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Points)
            .IsRequired();

        builder.Property(x => x.MonetaryValue)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Reference)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.Reference)
            .IsUnique();
    }
}
