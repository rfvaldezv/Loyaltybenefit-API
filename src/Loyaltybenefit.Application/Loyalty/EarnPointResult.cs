namespace Loyaltybenefit.Application.Loyalty;

public sealed record EarnPointsResult(
    Guid LoyaltyAccountId,
    Guid GuestId,
    Guid StayId,
    int Points,
    int PointsBalance);
    