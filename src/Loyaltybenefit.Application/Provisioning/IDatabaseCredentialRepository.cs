using Loyaltybenefit.Domain.Entities;

namespace Loyaltybenefit.Application.Provisioning;

public interface IDatabaseCredentialRepository
{
    Task<DatabaseCredential?> GetByDatabaseServerIdAsync(
        Guid databaseServerId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        DatabaseCredential credential,
        CancellationToken cancellationToken = default);
}
