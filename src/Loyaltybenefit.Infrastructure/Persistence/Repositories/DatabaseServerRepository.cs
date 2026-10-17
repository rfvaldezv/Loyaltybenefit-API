using Loyaltybenefit.Application.Provisioning;
using Loyaltybenefit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Loyaltybenefit.Infrastructure.Persistence.Repositories;

public sealed class DatabaseServerRepository
    : IDatabaseServerRepository
{
    private readonly PlatformDbContext _context;

    public DatabaseServerRepository(
        PlatformDbContext context)
    {
        _context = context;
    }

    public Task<DatabaseServer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _context.DatabaseServers
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public Task<DatabaseServer?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return _context.DatabaseServers
            .FirstOrDefaultAsync(
                x => x.Code == code,
                cancellationToken);
    }

    public async Task AddAsync(
        DatabaseServer databaseServer,
        CancellationToken cancellationToken = default)
    {
        await _context.DatabaseServers.AddAsync(
            databaseServer,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);
    }
}
