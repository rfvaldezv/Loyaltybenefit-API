namespace Loyaltybenefit.Infrastructure.Domain.Loyalty;

public class LoyaltyAccount
{
    public Guid Id { get; set; }

    public Guid GuestId { get; set; }

    public int PointsBalance { get; set; }

    public decimal MonetaryBalance { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
