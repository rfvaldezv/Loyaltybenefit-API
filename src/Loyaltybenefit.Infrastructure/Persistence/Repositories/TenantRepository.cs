using Loyaltybenefit.Application.Tenants;
using Loyaltybenefit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Loyaltybenefit.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly PlatformDbContext _context;

    public TenantRepository(PlatformDbContext context)
    {
        _context = context;
    }

    public Task<Tenant?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _context.Tenants
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public Task<Tenant?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return _context.Tenants
            .FirstOrDefaultAsync(
                x => x.Code == code,
                cancellationToken);
    }

    public async Task AddAsync(
        Tenant tenant,
        CancellationToken cancellationToken = default)
    {
        await _context.Tenants.AddAsync(
            tenant,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);
    }
}
