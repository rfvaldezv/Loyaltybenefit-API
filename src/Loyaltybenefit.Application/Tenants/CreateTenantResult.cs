namespace Loyaltybenefit.Application.Tenants;

public sealed record CreateTenantResult(
    Guid Id,
    string Code,
    string Name,
    string IndustryCode);
    