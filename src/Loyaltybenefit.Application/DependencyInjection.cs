using Loyaltybenefit.Application.Provisioning;
using Loyaltybenefit.Application.Tenants;
using Microsoft.Extensions.DependencyInjection;
using Loyaltybenefit.Application.Loyalty;

namespace Loyaltybenefit.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateTenantService>();

        services.AddScoped<CreateTenantDatabaseService>();

        services.AddScoped<EarnPointsService>();

        services.AddScoped<RedeemPointsService>();

        return services;
    }
}
