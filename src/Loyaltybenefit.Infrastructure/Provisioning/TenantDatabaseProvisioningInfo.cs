using Loyaltybenefit.Application.Provisioning;
using Microsoft.Extensions.Configuration;

namespace Loyaltybenefit.Infrastructure.Provisioning;

public sealed class TenantDatabaseProvisioningInfo
    : ITenantDatabaseProvisioningInfo
{
    private readonly IConfiguration _configuration;

    public TenantDatabaseProvisioningInfo(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string ServerName =>
        _configuration[
            "DatabaseProvisioning:Site4Now:Host"]
        ?? throw new InvalidOperationException(
            "DatabaseProvisioning:Site4Now:Host no está configurado.");

    public int Port
    {
        get
        {
            var value =
                _configuration[
                    "DatabaseProvisioning:Site4Now:Port"];

            if (!int.TryParse(
                    value,
                    out var port))
            {
                throw new InvalidOperationException(
                    "DatabaseProvisioning:Site4Now:Port no está configurado correctamente.");
            }

            return port;
        }
    }
}
