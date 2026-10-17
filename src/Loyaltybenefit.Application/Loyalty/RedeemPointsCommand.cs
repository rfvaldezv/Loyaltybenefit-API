namespace Loyaltybenefit.Application.Loyalty;

public sealed record RedeemPointsCommand(
    Guid TenantId,
    Guid GuestId,
    int Points,
    string Reference);
