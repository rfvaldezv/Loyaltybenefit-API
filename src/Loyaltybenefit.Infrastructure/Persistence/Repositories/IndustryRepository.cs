using Loyaltybenefit.Application.Tenants;
using Loyaltybenefit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Loyaltybenefit.Infrastructure.Persistence.Repositories;

public sealed class IndustryRepository : IIndustryRepository
{
    private readonly PlatformDbContext _context;

    public IndustryRepository(PlatformDbContext context)
    {
        _context = context;
    }

    public Task<Industry?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return _context.Industries
            .FirstOrDefaultAsync(
                x => x.Code == code,
                cancellationToken);
    }
}
