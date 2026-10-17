namespace Loyaltybenefit.Domain.Entities;

public sealed class LoyaltyAccount
{
    public Guid Id { get; private set; }

    public Guid GuestId { get; private set; }

    public int PointsBalance { get; private set; }

    public decimal MonetaryBalance { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private LoyaltyAccount()
    {
    }

    public LoyaltyAccount(
        Guid id,
        Guid guestId)
    {
        Id = id;
        GuestId = guestId;
        PointsBalance = 0;
        MonetaryBalance = 0m;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void AddPoints(int points)
    {
        if (points <= 0)
        {
            throw new ArgumentException(
                "Points to add must be greater than zero.",
                nameof(points));
        }

        PointsBalance += points;
    }

    public void RemovePoints(int points)
    {
        if (points <= 0)
        {
            throw new ArgumentException(
                "Points to remove must be greater than zero.",
                nameof(points));
        }

        if (points > PointsBalance)
        {
            throw new InvalidOperationException(
                "Insufficient points balance.");
        }

        PointsBalance -= points;
    }

    public void SetMonetaryBalance(
        decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentException(
                "Monetary balance cannot be negative.",
                nameof(amount));
        }

        MonetaryBalance = amount;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
