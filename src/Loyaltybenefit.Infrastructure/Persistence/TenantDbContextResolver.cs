using Loyaltybenefit.Application.Loyalty;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Loyaltybenefit.Infrastructure.Persistence;

public sealed class TenantDbContextResolver
{
    private readonly PlatformDbContext _platformContext;

    public TenantDbContextResolver(
        PlatformDbContext platformContext)
    {
        _platformContext = platformContext;
    }

    public async Task<TenantDbContext> CreateAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var tenantDatabase =
            await _platformContext.TenantDatabases
                .FirstOrDefaultAsync(
                    x => x.TenantId == tenantId &&
                         x.IsActive,
                    cancellationToken);

        if (tenantDatabase is null)
        {
            throw new InvalidOperationException(
                $"No existe una base de datos activa para el Tenant '{tenantId}'.");
        }

        var server =
            await _platformContext.DatabaseServers
                .FirstOrDefaultAsync(
                    x => x.Id == tenantDatabase.DatabaseServerId &&
                         x.IsActive,
                    cancellationToken);

        if (server is null)
        {
            throw new InvalidOperationException(
                $"No existe un DatabaseServer activo para el Tenant '{tenantId}'.");
        }

        var credential =
            await _platformContext.DatabaseCredentials
                .FirstOrDefaultAsync(
                    x => x.DatabaseServerId == server.Id &&
                         x.IsActive,
                    cancellationToken);

        if (credential is null)
        {
            throw new InvalidOperationException(
                $"No existe una credencial activa para el servidor '{server.Code}'.");
        }

        var connectionBuilder =
            new SqlConnectionStringBuilder
            {
                DataSource =
                    $"{server.Host},{server.Port}",

                InitialCatalog =
                    tenantDatabase.PhysicalDatabaseName,

                UserID =
                    credential.Username,

                Password =
                    credential.PasswordSecret,

                Encrypt = true,

                TrustServerCertificate = true
            };

        var options =
            new DbContextOptionsBuilder<TenantDbContext>()
                .UseSqlServer(
                    connectionBuilder.ConnectionString)
                .Options;

        return new TenantDbContext(options);
    }
}
