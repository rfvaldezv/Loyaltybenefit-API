using Loyaltybenefit.Domain.Entities;

namespace Loyaltybenefit.Application.Provisioning;

public interface IDatabaseServerRepository
{
    Task<DatabaseServer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<DatabaseServer?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        DatabaseServer databaseServer,
        CancellationToken cancellationToken = default);
}
