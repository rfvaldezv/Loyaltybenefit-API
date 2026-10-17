using Loyaltybenefit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loyaltybenefit.Infrastructure.Persistence.Configurations;

public sealed class LoyaltyRuleConfiguration
    : IEntityTypeConfiguration<LoyaltyRule>
{
    public void Configure(
        EntityTypeBuilder<LoyaltyRule> builder)
    {
        builder.ToTable("LoyaltyRules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.AmountThreshold)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Points)
            .IsRequired();

        builder.Property(x => x.RedemptionPoints)
            .IsRequired();

        builder.Property(x => x.RedemptionAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();
    }
}
