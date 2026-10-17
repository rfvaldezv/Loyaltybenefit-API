namespace Loyaltybenefit.Application.Loyalty;

public interface ITenantDbContextFactory
{
    Task<IDisposable> CreateAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
