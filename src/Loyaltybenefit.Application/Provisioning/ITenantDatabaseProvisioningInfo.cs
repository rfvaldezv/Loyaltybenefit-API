namespace Loyaltybenefit.Application.Provisioning;

public interface ITenantDatabaseProvisioningInfo
{
    string ServerName { get; }

    int Port { get; }
}
