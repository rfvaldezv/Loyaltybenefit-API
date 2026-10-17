using Loyaltybenefit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loyaltybenefit.Infrastructure.Persistence.Configurations;

public sealed class LoyaltyAccountConfiguration
    : IEntityTypeConfiguration<LoyaltyAccount>
{
    public void Configure(
        EntityTypeBuilder<LoyaltyAccount> builder)
    {
        builder.ToTable("LoyaltyAccounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.GuestId)
            .IsRequired();

        builder.HasIndex(x => x.GuestId)
            .IsUnique();

        builder.HasOne<Guest>()
            .WithOne()
            .HasForeignKey<LoyaltyAccount>(
                x => x.GuestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.PointsBalance)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();
    }
}
