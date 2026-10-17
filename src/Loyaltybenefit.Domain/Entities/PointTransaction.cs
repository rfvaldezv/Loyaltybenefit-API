namespace Loyaltybenefit.Domain.Entities;

public sealed class PointTransaction
{
    public Guid Id { get; private set; }

    public Guid LoyaltyAccountId { get; private set; }

    public Guid? StayId { get; private set; }

    public int Points { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? Reference { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private PointTransaction()
    {
    }

    public PointTransaction(
        Guid id,
        Guid loyaltyAccountId,
        int points,
        string type,
        string? description = null,
        string? reference = null,
        Guid? stayId = null)
    {
        if (points == 0)
        {
            throw new ArgumentException(
                "Points cannot be zero.",
                nameof(points));
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException(
                "Transaction type is required.",
                nameof(type));
        }

        Id = id;
        LoyaltyAccountId = loyaltyAccountId;
        StayId = stayId;
        Points = points;
        Type = type;
        Description = description;
        Reference = reference;
        CreatedAtUtc = DateTime.UtcNow;
    }
}
