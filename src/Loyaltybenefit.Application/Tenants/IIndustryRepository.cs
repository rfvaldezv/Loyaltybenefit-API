using Loyaltybenefit.Domain.Entities;

namespace Loyaltybenefit.Application.Tenants;

public interface IIndustryRepository
{
    Task<Industry?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);
}
