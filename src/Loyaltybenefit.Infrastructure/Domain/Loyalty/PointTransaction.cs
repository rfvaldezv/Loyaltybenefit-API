namespace Loyaltybenefit.Infrastructure.Domain.Loyalty;

public class PointTransaction
{
    public Guid Id { get; set; }

    public Guid LoyaltyAccountId { get; set; }

    public int Points { get; set; }

    public string Type { get; set; } = null!;

    public string? Description { get; set; }

    public string? Reference { get; set; }

    public DateTime CreatedAt { get; set; }
}
