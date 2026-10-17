namespace Loyaltybenefit.Application.Loyalty;

public sealed record EarnPointsCommand(
    Guid TenantId,
    Guid GuestId,
    Guid StayId);
