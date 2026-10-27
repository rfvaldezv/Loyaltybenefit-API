using Loyaltybenefit.Application.Loyalty;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Loyaltybenefit.Infrastructure.Persistence;

public sealed class TenantDbContextFactory
{
    private readonly PlatformDbContext _platformContext;
    private readonly ITenantContext _tenantContext;

    private TenantDbContext? _context;

    public TenantDbContextFactory(
        PlatformDbContext platformContext,
        ITenantContext tenantContext)
    {
        _platformContext = platformContext;
        _tenantContext = tenantContext;
    }

    public async Task<TenantDbContext> CreateAsync(
        CancellationToken cancellationToken = default)
    {
        if (_context is not null)
        {
            return _context;
        }

        if (_tenantContext.TenantId == Guid.Empty)
        {
            throw new InvalidOperationException("No se ha establecido el Tenant actual.");
        }

        var tenantDatabase =
            await _platformContext.TenantDatabases
                .FirstOrDefaultAsync(
                    x => x.TenantId == _tenantContext.TenantId && x.IsActive,
                    cancellationToken);

        if (tenantDatabase is null)
            throw new InvalidOperationException($"No existe BD activa para el Tenant '{_tenantContext.TenantId}'.");

        var databaseServer =
            await _platformContext.DatabaseServers
                .FirstOrDefaultAsync(
                    x => x.Id == tenantDatabase.DatabaseServerId && x.IsActive,
                    cancellationToken);

        if (databaseServer is null)
            throw new InvalidOperationException($"No existe servidor activo para el Tenant '{_tenantContext.TenantId}'.");

        var credential =
            await _platformContext.DatabaseCredentials
                .FirstOrDefaultAsync(
                    x => x.DatabaseServerId == databaseServer.Id && x.IsActive,
                    cancellationToken);

        if (credential is null)
            throw new InvalidOperationException($"No existe credencial activa para el servidor '{databaseServer.Code}'.");

        var connectionBuilder =
            new SqlConnectionStringBuilder
            {
                DataSource = $"{databaseServer.Host},{databaseServer.Port}",
                InitialCatalog = tenantDatabase.PhysicalDatabaseName,
                UserID = credential.Username,
                Password = credential.PasswordSecret,
                Encrypt = true,
                TrustServerCertificate = true
            };

        var options =
            new DbContextOptionsBuilder<TenantDbContext>()
                .UseSqlServer(
                    connectionBuilder.ConnectionString,
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(TenantDbContext).Assembly.FullName);
                        sqlOptions.EnableRetryOnFailure(0);
                        sqlOptions.CommandTimeout(30);
                    })
                .Options;

        _context = new TenantDbContext(options);

        return _context;
    }
}
