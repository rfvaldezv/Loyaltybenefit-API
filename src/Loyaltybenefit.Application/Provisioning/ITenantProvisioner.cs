namespace Loyaltybenefit.Application.Provisioning;

public interface ITenantProvisioner
{
    Task ProvisionAsync(
        Guid tenantDatabaseId,
        CancellationToken cancellationToken = default);
}
