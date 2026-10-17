using Loyaltybenefit.Application.Loyalty;
using Loyaltybenefit.Application.Provisioning;
using Loyaltybenefit.Application.Tenants;
using Loyaltybenefit.Infrastructure.Persistence;
using Loyaltybenefit.Infrastructure.Persistence.Repositories;
using Loyaltybenefit.Infrastructure.Provisioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Loyaltybenefit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("Platform");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString =
                Environment.GetEnvironmentVariable(
                    "Loyaltybenefit_PLATFORM_CONNECTION");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Platform database connection is not configured.");
        }

        // ---------------------------------------------------------
        // Platform database
        // ---------------------------------------------------------

        services.AddDbContext<PlatformDbContext>(
            options =>
            {
                options.UseSqlServer(
                    connectionString);
            });

        // ---------------------------------------------------------
        // Platform repositories
        // ---------------------------------------------------------

        services.AddScoped<
            ITenantRepository,
            TenantRepository>();

        services.AddScoped<
            IIndustryRepository,
            IndustryRepository>();

        services.AddScoped<
            ITenantDatabaseRepository,
            TenantDatabaseRepository>();

        services.AddScoped<
            IDatabaseServerRepository,
            DatabaseServerRepository>();

        services.AddScoped<
            IDatabaseCredentialRepository,
            DatabaseCredentialRepository>();

        // ---------------------------------------------------------
        // Tenant context
        // ---------------------------------------------------------

        services.AddScoped<
            ITenantContext,
            TenantContext>();

        services.AddScoped<
            TenantDbContextFactory>();

        // ---------------------------------------------------------
        // Tenant provisioning
        // ---------------------------------------------------------

        services.AddSingleton<
            ITenantDatabaseProvisioningInfo,
            TenantDatabaseProvisioningInfo>();

        services.AddScoped<
            ITenantProvisioner,
            SqlServerTenantProvisioner>();

        // ---------------------------------------------------------
        // Loyalty
        // ---------------------------------------------------------

        services.AddScoped<
            ILoyaltyRepository,
            LoyaltyRepository>();

        return services;
    }
}
