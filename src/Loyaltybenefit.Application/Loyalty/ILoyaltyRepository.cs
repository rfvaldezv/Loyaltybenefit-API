using Loyaltybenefit.Domain.Entities;

namespace Loyaltybenefit.Application.Loyalty;

public interface ILoyaltyRepository
{
    Task<Guest?> GetGuestAsync(
        Guid guestId,
        CancellationToken cancellationToken = default);

    Task<Stay?> GetStayAsync(
        Guid stayId,
        CancellationToken cancellationToken = default);

    Task<LoyaltyAccount?> GetAccountByGuestIdAsync(
        Guid guestId,
        CancellationToken cancellationToken = default);

    Task<LoyaltyRule?> GetActiveRuleAsync(
        CancellationToken cancellationToken = default);

    Task<PointTransaction?> GetStayEarnTransactionAsync(
        Guid stayId,
        CancellationToken cancellationToken = default);

    Task AddAccountAsync(
        LoyaltyAccount account,
        CancellationToken cancellationToken = default);

    Task AddTransactionAsync(
        PointTransaction transaction,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<Redemption?> GetRedemptionByReferenceAsync(
        string reference,
        CancellationToken cancellationToken = default);

    Task AddRedemptionAsync(
        Redemption redemption,
        CancellationToken cancellationToken = default);
}
