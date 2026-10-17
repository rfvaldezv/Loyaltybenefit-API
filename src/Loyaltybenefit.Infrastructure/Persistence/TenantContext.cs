using Loyaltybenefit.Application.Loyalty;

namespace Loyaltybenefit.Infrastructure.Persistence;

public sealed class TenantContext : ITenantContext
{
    public Guid TenantId { get; private set; }

    public void SetTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException(
                "TenantId no puede ser vacío.",
                nameof(tenantId));
        }

        TenantId = tenantId;
    }
}
