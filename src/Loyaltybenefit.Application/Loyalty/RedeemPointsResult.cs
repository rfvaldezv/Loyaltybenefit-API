namespace Loyaltybenefit.Application.Loyalty;

public sealed record RedeemPointsResult(
    Guid RedemptionId,
    Guid LoyaltyAccountId,
    Guid GuestId,
    int PointsRedeemed,
    decimal MonetaryValue,
    int PointsBalance,
    string Reference);
    