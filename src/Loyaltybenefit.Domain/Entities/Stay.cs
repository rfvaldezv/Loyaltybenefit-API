namespace Loyaltybenefit.Domain.Entities;

public sealed class Stay
{
    public Guid Id { get; private set; }

    public Guid HotelId { get; private set; }

    public Guid GuestId { get; private set; }

    public string? ReservationNumber { get; private set; }

    public DateTime CheckInUtc { get; private set; }

    public DateTime CheckOutUtc { get; private set; }

    public int Nights { get; private set; }

    public decimal Amount { get; private set; }

    public string Status { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    private Stay()
    {
    }

    public Stay(
        Guid id,
        Guid hotelId,
        Guid guestId,
        DateTime checkInUtc,
        DateTime checkOutUtc,
        decimal amount,
        string? reservationNumber = null)
    {
        if (checkOutUtc <= checkInUtc)
        {
            throw new ArgumentException(
                "Check-out must be after check-in.",
                nameof(checkOutUtc));
        }

        if (amount < 0)
        {
            throw new ArgumentException(
                "Amount cannot be negative.",
                nameof(amount));
        }

        Id = id;
        HotelId = hotelId;
        GuestId = guestId;
        ReservationNumber = reservationNumber;
        CheckInUtc = checkInUtc;
        CheckOutUtc = checkOutUtc;
        Nights = CalculateNights(
            checkInUtc,
            checkOutUtc);
        Amount = amount;
        Status = "CONFIRMED";
        CreatedAtUtc = DateTime.UtcNow;
    }

    private static int CalculateNights(
        DateTime checkInUtc,
        DateTime checkOutUtc)
    {
        return (checkOutUtc.Date - checkInUtc.Date).Days;
    }

    public void CheckIn()
    {
        Status = "CHECKED_IN";
    }

    public void CheckOut()
    {
        Status = "COMPLETED";
    }

    public void Cancel()
    {
        Status = "CANCELLED";
    }
}
