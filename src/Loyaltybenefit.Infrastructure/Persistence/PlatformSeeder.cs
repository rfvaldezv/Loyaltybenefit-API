using Loyaltybenefit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Loyaltybenefit.Infrastructure.Persistence;

public static class PlatformSeeder
{
    public static async Task SeedAsync(
        PlatformDbContext context,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        await SeedIndustryAsync(
            context,
            cancellationToken);

        await SeedDatabaseServerAsync(
            context,
            configuration,
            cancellationToken);
    }

    private static async Task SeedIndustryAsync(
        PlatformDbContext context,
        CancellationToken cancellationToken)
    {
        if (await context.Industries.AnyAsync(
                x => x.Code == "HOTEL",
                cancellationToken))
        {
            return;
        }

        var hotel = new Industry(
            Guid.NewGuid(),
            "HOTEL",
            "Hoteles");

        await context.Industries.AddAsync(
            hotel,
            cancellationToken);

        await context.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedDatabaseServerAsync(
        PlatformDbContext context,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        const string serverCode = "SITE4NOW01";

        var server =
            await context.DatabaseServers
                .FirstOrDefaultAsync(
                    x => x.Code == serverCode,
                    cancellationToken);

        if (server is null)
        {
            var host =
                configuration[
                    "DatabaseProvisioning:Site4Now:Host"];

            var portValue =
                configuration[
                    "DatabaseProvisioning:Site4Now:Port"];

            if (string.IsNullOrWhiteSpace(host))
            {
                host =
                    Environment.GetEnvironmentVariable(
                        "Loyaltybenefit_SITE4NOW_HOST");
            }

            if (!int.TryParse(
                    portValue,
                    out var port))
            {
                port = 1433;
            }

            if (string.IsNullOrWhiteSpace(host))
            {
                throw new InvalidOperationException(
                    "Site4Now Host no está configurado.");
            }

            server = new DatabaseServer(
                Guid.NewGuid(),
                serverCode,
                "Site4Now SQL Server",
                "SITE4NOW",
                "SQLSERVER",
                host,
                port);

            await context.DatabaseServers.AddAsync(
                server,
                cancellationToken);

            await context.SaveChangesAsync(
                cancellationToken);
        }

        var username =
            configuration[
                "DatabaseProvisioning:Site4Now:Username"];

        var password =
            configuration[
                "DatabaseProvisioning:Site4Now:Password"];

        if (string.IsNullOrWhiteSpace(username))
        {
            username =
                Environment.GetEnvironmentVariable(
                    "Loyaltybenefit_SITE4NOW_USERNAME");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            password =
                Environment.GetEnvironmentVariable(
                    "Loyaltybenefit_SITE4NOW_PASSWORD");
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidOperationException(
                "Site4Now Username no está configurado.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Site4Now Password no está configurado.");
        }

        var credentialExists =
            await context.DatabaseCredentials
                .AnyAsync(
                    x =>
                        x.DatabaseServerId == server.Id &&
                        x.IsActive,
                    cancellationToken);

        if (credentialExists)
        {
            return;
        }

        var credential = new DatabaseCredential(
            Guid.NewGuid(),
            server.Id,
            username,
            password);

        await context.DatabaseCredentials.AddAsync(
            credential,
            cancellationToken);

        await context.SaveChangesAsync(
            cancellationToken);
    }
}
