using Loyaltybenefit.Domain.Entities;

namespace Loyaltybenefit.Application.Provisioning;

public interface ITenantDatabaseRepository
{
    Task<TenantDatabase?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<TenantDatabase?> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        TenantDatabase tenantDatabase,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        TenantDatabase tenantDatabase,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TenantDatabase>> GetPendingAsync(
        CancellationToken cancellationToken = default);

}
