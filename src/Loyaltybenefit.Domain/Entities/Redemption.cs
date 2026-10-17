namespace Loyaltybenefit.Domain.Entities;

public sealed class Redemption
{
    public Guid Id { get; private set; }

    public Guid LoyaltyAccountId { get; private set; }

    public int Points { get; private set; }

    public decimal MonetaryValue { get; private set; }

    public string Reference { get; private set; } = string.Empty;

    public string Status { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    private Redemption()
    {
    }

    public Redemption(
        Guid id,
        Guid loyaltyAccountId,
        int points,
        decimal monetaryValue,
        string reference)
    {
        if (points <= 0)
        {
            throw new ArgumentException(
                "Points must be greater than zero.",
                nameof(points));
        }

        if (monetaryValue <= 0)
        {
            throw new ArgumentException(
                "Monetary value must be greater than zero.",
                nameof(monetaryValue));
        }

        if (string.IsNullOrWhiteSpace(reference))
        {
            throw new ArgumentException(
                "Reference is required.",
                nameof(reference));
        }

        Id = id;
        LoyaltyAccountId = loyaltyAccountId;
        Points = points;
        MonetaryValue = monetaryValue;
        Reference = reference;
        Status = "COMPLETED";
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == "CANCELLED")
        {
            return;
        }

        Status = "CANCELLED";
    }
}
