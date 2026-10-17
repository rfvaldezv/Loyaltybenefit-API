using Loyaltybenefit.Application.Provisioning;
using Loyaltybenefit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Loyaltybenefit.Infrastructure.Persistence.Repositories;

public sealed class DatabaseCredentialRepository
    : IDatabaseCredentialRepository
{
    private readonly PlatformDbContext _context;

    public DatabaseCredentialRepository(
        PlatformDbContext context)
    {
        _context = context;
    }

    public Task<DatabaseCredential?> GetByDatabaseServerIdAsync(
        Guid databaseServerId,
        CancellationToken cancellationToken = default)
    {
        return _context.DatabaseCredentials
            .FirstOrDefaultAsync(
                x =>
                    x.DatabaseServerId == databaseServerId &&
                    x.IsActive,
                cancellationToken);
    }

    public async Task AddAsync(
        DatabaseCredential credential,
        CancellationToken cancellationToken = default)
    {
        await _context.DatabaseCredentials.AddAsync(
            credential,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);
    }
}
