namespace Loyaltybenefit.Application.Loyalty;

public sealed class RedeemPointsService
{
    private readonly ILoyaltyRepository _repository;
    private readonly ITenantContext _tenantContext;

    public RedeemPointsService(
        ILoyaltyRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<RedeemPointsResult> ExecuteAsync(
        RedeemPointsCommand command,
        CancellationToken cancellationToken = default)
    {
        _tenantContext.SetTenant(command.TenantId);

        if (command.Points <= 0)
        {
            throw new ArgumentException(
                "Points to redeem must be greater than zero.",
                nameof(command.Points));
        }

        if (string.IsNullOrWhiteSpace(command.Reference))
        {
            throw new ArgumentException(
                "Reference is required.",
                nameof(command.Reference));
        }

        var existingRedemption =
            await _repository.GetRedemptionByReferenceAsync(
                command.Reference,
                cancellationToken);

        if (existingRedemption is not null)
        {
            var existingAccount =
                await _repository.GetAccountByGuestIdAsync(
                    command.GuestId,
                    cancellationToken);

            if (existingAccount is null)
            {
                throw new InvalidOperationException(
                    "Loyalty account was not found.");
            }

            return new RedeemPointsResult(
                existingRedemption.Id,
                existingRedemption.LoyaltyAccountId,
                command.GuestId,
                existingRedemption.Points,
                existingRedemption.MonetaryValue,
                existingAccount.PointsBalance,
                existingRedemption.Reference);
        }

        var guest =
            await _repository.GetGuestAsync(
                command.GuestId,
                cancellationToken);

        if (guest is null)
        {
            throw new InvalidOperationException(
                $"Guest '{command.GuestId}' was not found.");
        }

        if (!guest.IsActive)
        {
            throw new InvalidOperationException(
                "Guest is not active.");
        }

        var account =
            await _repository.GetAccountByGuestIdAsync(
                guest.Id,
                cancellationToken);

        if (account is null)
        {
            throw new InvalidOperationException(
                "Guest does not have a loyalty account.");
        }

        var rule =
            await _repository.GetActiveRuleAsync(
                cancellationToken);

        if (rule is null)
        {
            throw new InvalidOperationException(
                "No active loyalty rule was found.");
        }

        var monetaryValue =
            CalculateRedemptionValue(
                command.Points,
                rule);

        account.RemovePoints(command.Points);

        var redemption = new Domain.Entities.Redemption(
            Guid.NewGuid(),
            account.Id,
            command.Points,
            monetaryValue,
            command.Reference);

        var transaction =
            new Domain.Entities.PointTransaction(
                Guid.NewGuid(),
                account.Id,
                -command.Points,
                "REDEMPTION",
                $"Points redeemed for {monetaryValue:C}.",
                command.Reference);

        await _repository.AddRedemptionAsync(
            redemption,
            cancellationToken);

        await _repository.AddTransactionAsync(
            transaction,
            cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return new RedeemPointsResult(
            redemption.Id,
            account.Id,
            guest.Id,
            command.Points,
            monetaryValue,
            account.PointsBalance,
            redemption.Reference);
    }

    private static decimal CalculateRedemptionValue(
        int points,
        Domain.Entities.LoyaltyRule rule)
    {
        var blocks =
            decimal.Floor(
                (decimal)points / rule.RedemptionPoints);

        return blocks * rule.RedemptionAmount;
    }
}
