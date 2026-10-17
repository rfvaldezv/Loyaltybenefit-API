using Loyaltybenefit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loyaltybenefit.Infrastructure.Persistence.Configurations;

public sealed class PointTransactionConfiguration
    : IEntityTypeConfiguration<PointTransaction>
{
    public void Configure(
        EntityTypeBuilder<PointTransaction> builder)
    {
        builder.ToTable("PointTransactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LoyaltyAccountId)
            .IsRequired();

        builder.HasIndex(x => x.LoyaltyAccountId);

        builder.HasOne<LoyaltyAccount>()
            .WithMany()
            .HasForeignKey(x => x.LoyaltyAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.StayId);

        builder.HasIndex(x => x.StayId);

        builder.HasIndex(x => x.StayId)
            .IsUnique()
            .HasFilter("[StayId] IS NOT NULL AND [Type] = 'STAY_EARN'");

        builder.HasOne<Stay>()
            .WithMany()
            .HasForeignKey(x => x.StayId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Points)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Reference)
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();
    }
}
