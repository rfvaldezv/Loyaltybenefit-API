using Loyaltybenefit.Domain.Entities;

namespace Loyaltybenefit.Application.Tenants;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Tenant?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Tenant tenant,
        CancellationToken cancellationToken = default);
}
