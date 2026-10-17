using Loyaltybenefit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loyaltybenefit.Infrastructure.Persistence.Configurations;

public sealed class StayConfiguration
    : IEntityTypeConfiguration<Stay>
{
    public void Configure(
        EntityTypeBuilder<Stay> builder)
    {
        builder.ToTable("Stays");

        builder.HasKey(x => x.Id);

        // --------------------------------------------------------
        // Hotel
        // --------------------------------------------------------

        builder.Property(x => x.HotelId)
            .IsRequired();

        builder.HasIndex(x => x.HotelId);

        builder.HasOne<Hotel>()
            .WithMany()
            .HasForeignKey(x => x.HotelId)
            .OnDelete(DeleteBehavior.Restrict);


        // --------------------------------------------------------
        // Guest
        // --------------------------------------------------------

        builder.Property(x => x.GuestId)
            .IsRequired();

        builder.HasIndex(x => x.GuestId);

        builder.HasOne<Guest>()
            .WithMany()
            .HasForeignKey(x => x.GuestId)
            .OnDelete(DeleteBehavior.Restrict);


        // --------------------------------------------------------
        // Reservation
        // --------------------------------------------------------

        builder.Property(x => x.ReservationNumber)
            .HasMaxLength(100);

        builder.HasIndex(x => x.ReservationNumber);


        // --------------------------------------------------------
        // Dates
        // --------------------------------------------------------

        builder.Property(x => x.CheckInUtc)
            .IsRequired();

        builder.Property(x => x.CheckOutUtc)
            .IsRequired();


        // --------------------------------------------------------
        // Nights
        // --------------------------------------------------------

        builder.Property(x => x.Nights)
            .IsRequired();


        // --------------------------------------------------------
        // Amount
        // --------------------------------------------------------

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();


        // --------------------------------------------------------
        // Status
        // --------------------------------------------------------

        builder.Property(x => x.Status)
            .HasMaxLength(30)
            .IsRequired();


        // --------------------------------------------------------
        // Audit
        // --------------------------------------------------------

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();
    }
}
