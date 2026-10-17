using Loyaltybenefit.Application.Loyalty;
using Loyaltybenefit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Loyaltybenefit.Infrastructure.Persistence.Repositories;

public sealed class LoyaltyRepository : ILoyaltyRepository
{
    private readonly TenantDbContextFactory _contextFactory;

    private TenantDbContext? _context;

    public LoyaltyRepository(
        TenantDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    private async Task<TenantDbContext> GetContextAsync(
        CancellationToken cancellationToken)
    {
        _context ??=
            await _contextFactory.CreateAsync(
                cancellationToken);

        return _context;
    }

    public async Task<Guest?> GetGuestAsync(
        Guid guestId,
        CancellationToken cancellationToken = default)
    {
        var context =
            await GetContextAsync(
                cancellationToken);

        return await context.Guests
            .FindAsync(
                [guestId],
                cancellationToken);
    }

    public async Task<Stay?> GetStayAsync(
        Guid stayId,
        CancellationToken cancellationToken = default)
    {
        var context =
            await GetContextAsync(
                cancellationToken);

        return await context.Stays
            .FindAsync(
                [stayId],
                cancellationToken);
    }

    public async Task<LoyaltyAccount?> GetAccountByGuestIdAsync(
        Guid guestId,
        CancellationToken cancellationToken = default)
    {
        var context =
            await GetContextAsync(
                cancellationToken);

        return await context.LoyaltyAccounts
            .FirstOrDefaultAsync(
                x => x.GuestId == guestId,
                cancellationToken);
    }

    public async Task<LoyaltyRule?> GetActiveRuleAsync(
        CancellationToken cancellationToken = default)
    {
        var context =
            await GetContextAsync(
                cancellationToken);

        return await context.LoyaltyRules
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .FirstOrDefaultAsync(
                cancellationToken);
    }

    public async Task<PointTransaction?> GetStayEarnTransactionAsync(
        Guid stayId,
        CancellationToken cancellationToken = default)
    {
        var context =
            await GetContextAsync(
                cancellationToken);

        return await context.PointTransactions
            .FirstOrDefaultAsync(
                x =>
                    x.StayId == stayId &&
                    x.Type == "STAY_EARN",
                cancellationToken);
    }

    public async Task AddAccountAsync(
        LoyaltyAccount account,
        CancellationToken cancellationToken = default)
    {
        var context =
            await GetContextAsync(
                cancellationToken);

        await context.LoyaltyAccounts.AddAsync(
            account,
            cancellationToken);
    }

    public async Task AddTransactionAsync(
        PointTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        var context =
            await GetContextAsync(
                cancellationToken);

        await context.PointTransactions.AddAsync(
            transaction,
            cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var context =
            await GetContextAsync(
                cancellationToken);

        await context.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<Redemption?> GetRedemptionByReferenceAsync(
        string reference,
        CancellationToken cancellationToken = default)
    {
        var context =
            await GetContextAsync(
                cancellationToken);

        return await context.Redemptions
            .FirstOrDefaultAsync(
                x => x.Reference == reference,
                cancellationToken);
    }

    public async Task AddRedemptionAsync(
        Redemption redemption,
        CancellationToken cancellationToken = default)
    {
        var context =
            await GetContextAsync(
                cancellationToken);

        await context.Redemptions.AddAsync(
            redemption,
            cancellationToken);
    }
}
