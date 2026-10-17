using Loyaltybenefit.Domain.Entities;

namespace Loyaltybenefit.Application.Tenants;

public sealed class CreateTenantService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IIndustryRepository _industryRepository;

    public CreateTenantService(
        ITenantRepository tenantRepository,
        IIndustryRepository industryRepository)
    {
        _tenantRepository = tenantRepository;
        _industryRepository = industryRepository;
    }

    public async Task<CreateTenantResult> ExecuteAsync(
        CreateTenantCommand command,
        CancellationToken cancellationToken = default)
    {
        var existingTenant =
            await _tenantRepository.GetByCodeAsync(
                command.Code,
                cancellationToken);

        if (existingTenant is not null)
        {
            throw new InvalidOperationException(
                $"Tenant '{command.Code}' already exists.");
        }

        var industry =
            await _industryRepository.GetByCodeAsync(
                command.IndustryCode,
                cancellationToken);

        if (industry is null)
        {
            throw new InvalidOperationException(
                $"Industry '{command.IndustryCode}' was not found.");
        }

        var tenant = new Tenant(
            Guid.NewGuid(),
            command.Code,
            command.Name,
            industry.Id);

        await _tenantRepository.AddAsync(
            tenant,
            cancellationToken);

        return new CreateTenantResult(
            tenant.Id,
            tenant.Code,
            tenant.Name,
            industry.Code);
    }
}
