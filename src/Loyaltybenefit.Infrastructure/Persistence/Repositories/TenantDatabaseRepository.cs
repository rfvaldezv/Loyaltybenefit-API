using Loyaltybenefit.Application.Provisioning;
using Loyaltybenefit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Loyaltybenefit.Domain.Enums;

namespace Loyaltybenefit.Infrastructure.Persistence.Repositories;

public sealed class TenantDatabaseRepository
    : ITenantDatabaseRepository
{
    private readonly PlatformDbContext _context;

    public TenantDatabaseRepository(
        PlatformDbContext context)
    {
        _context = context;
    }

    public Task<TenantDatabase?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _context.TenantDatabases
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public Task<TenantDatabase?> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return _context.TenantDatabases
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId,
                cancellationToken);
    }

    public async Task AddAsync(
        TenantDatabase tenantDatabase,
        CancellationToken cancellationToken = default)
    {
        await _context.TenantDatabases.AddAsync(
            tenantDatabase,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);
    }

    public async Task UpdateAsync(
        TenantDatabase tenantDatabase,
        CancellationToken cancellationToken = default)
    {
        _context.TenantDatabases.Update(tenantDatabase);

        await _context.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<IReadOnlyList<TenantDatabase>> GetPendingAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.TenantDatabases
            .Where(x => x.Status == TenantDatabaseStatus.Pending)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

}
