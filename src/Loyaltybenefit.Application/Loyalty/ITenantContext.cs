namespace Loyaltybenefit.Application.Loyalty;

public interface ITenantContext
{
    Guid TenantId { get; }

    void SetTenant(Guid tenantId);
}
