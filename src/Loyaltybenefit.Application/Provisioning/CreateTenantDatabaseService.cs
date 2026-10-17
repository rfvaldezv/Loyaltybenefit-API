using Loyaltybenefit.Application.Tenants;
using Loyaltybenefit.Domain.Entities;

namespace Loyaltybenefit.Application.Provisioning;

public sealed class CreateTenantDatabaseService
{
    private readonly ITenantRepository _tenantRepository;

    private readonly ITenantDatabaseRepository
        _tenantDatabaseRepository;

    private readonly IDatabaseServerRepository
        _databaseServerRepository;

    public CreateTenantDatabaseService(
        ITenantRepository tenantRepository,
        ITenantDatabaseRepository tenantDatabaseRepository,
        IDatabaseServerRepository databaseServerRepository)
    {
        _tenantRepository = tenantRepository;
        _tenantDatabaseRepository = tenantDatabaseRepository;
        _databaseServerRepository = databaseServerRepository;
    }

    public async Task<Guid> ExecuteAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var tenant =
            await _tenantRepository.GetByIdAsync(
                tenantId,
                cancellationToken);

        if (tenant is null)
        {
            throw new InvalidOperationException(
                $"Tenant '{tenantId}' was not found.");
        }

        var existing =
            await _tenantDatabaseRepository.GetByTenantIdAsync(
                tenantId,
                cancellationToken);

        if (existing is not null)
        {
            return existing.Id;
        }

        var databaseServer =
            await _databaseServerRepository.GetByCodeAsync(
                "SITE4NOW01",
                cancellationToken);

        if (databaseServer is null)
        {
            throw new InvalidOperationException(
                "DatabaseServer 'SITE4NOW01' was not found.");
        }

        var logicalName =
            $"Loyaltybenefit_T_{tenant.Code.Replace("-", "_")}";

        var physicalDatabaseName =
            $"loyaltybenefit{tenant.Code.Replace("-", "").ToLowerInvariant()}";

        var tenantDatabase = new TenantDatabase(
            Guid.NewGuid(),
            tenant.Id,
            databaseServer.Id,
            logicalName,
            physicalDatabaseName);

        await _tenantDatabaseRepository.AddAsync(
            tenantDatabase,
            cancellationToken);

        return tenantDatabase.Id;
    }
}
