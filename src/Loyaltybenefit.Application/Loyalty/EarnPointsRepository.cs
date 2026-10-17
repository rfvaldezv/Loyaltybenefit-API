using Loyaltybenefit.Domain.Entities;

namespace Loyaltybenefit.Application.Loyalty;

public sealed class EarnPointsService
{
    private readonly ILoyaltyRepository _repository;
    private readonly ITenantContext _tenantContext;

    public EarnPointsService(
        ILoyaltyRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<EarnPointsResult> ExecuteAsync(
        EarnPointsCommand command,
        CancellationToken cancellationToken = default)
    {
        _tenantContext.SetTenant(command.TenantId);

        var guest =
            await _repository.GetGuestAsync(
                command.GuestId,
                cancellationToken);

        if (guest is null)
        {
            throw new InvalidOperationException(
                $"Guest '{command.GuestId}' was not found.");
        }

        var stay =
            await _repository.GetStayAsync(
                command.StayId,
                cancellationToken);

        if (stay is null)
        {
            throw new InvalidOperationException(
                $"Stay '{command.StayId}' was not found.");
        }

        if (!guest.IsActive)
        {
            throw new InvalidOperationException(
                "Guest is not active.");
        }

        if (stay.GuestId != guest.Id)
        {
            throw new InvalidOperationException(
                "The stay does not belong to the specified guest.");
        }

        // ---------------------------------------------------------
        // Idempotencia:
        // una estancia solo puede generar puntos una vez.
        // ---------------------------------------------------------

        var existingTransaction =
            await _repository.GetStayEarnTransactionAsync(
                stay.Id,
                cancellationToken);

        if (existingTransaction is not null)
        {
            var existingAccount =
                await _repository.GetAccountByGuestIdAsync(
                    guest.Id,
                    cancellationToken);

            if (existingAccount is null)
            {
                throw new InvalidOperationException(
                    "Loyalty account was not found.");
            }

            return new EarnPointsResult(
                existingAccount.Id,
                guest.Id,
                stay.Id,
                existingTransaction.Points,
                existingAccount.PointsBalance);
        }

        var rule =
            await _repository.GetActiveRuleAsync(
                cancellationToken);

        if (rule is null)
        {
            throw new InvalidOperationException(
                "No active loyalty rule was found.");
        }

        var points =
            CalculatePoints(
                stay.Amount,
                rule);

        if (points <= 0)
        {
            throw new InvalidOperationException(
                "The stay did not generate loyalty points.");
        }

        var account =
            await _repository.GetAccountByGuestIdAsync(
                guest.Id,
                cancellationToken);

        if (account is null)
        {
            account = new LoyaltyAccount(
                Guid.NewGuid(),
                guest.Id);

            await _repository.AddAccountAsync(
                account,
                cancellationToken);
        }

        account.AddPoints(points);

        var transaction = new PointTransaction(
            Guid.NewGuid(),
            account.Id,
            points,
            "STAY_EARN",
            $"Points earned from stay {stay.Id}.",
            stay.ReservationNumber,
            stay.Id);

        await _repository.AddTransactionAsync(
            transaction,
            cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return new EarnPointsResult(
            account.Id,
            guest.Id,
            stay.Id,
            points,
            account.PointsBalance);
    }

    private static int CalculatePoints(
        decimal amount,
        LoyaltyRule rule)
    {
        if (amount <= 0)
        {
            return 0;
        }

        var blocks =
            decimal.Floor(
                amount / rule.AmountThreshold);

        var points =
            blocks * rule.Points;

        return checked((int)points);
    }
}
