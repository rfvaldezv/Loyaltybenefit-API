namespace Loyaltybenefit.Application.Tenants;

public sealed record CreateTenantCommand(
    string Code,
    string Name,
    string IndustryCode);
    