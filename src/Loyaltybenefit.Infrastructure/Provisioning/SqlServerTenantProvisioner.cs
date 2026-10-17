using Loyaltybenefit.Application.Provisioning;
using Loyaltybenefit.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Loyaltybenefit.Infrastructure.Provisioning;

public sealed class SqlServerTenantProvisioner
    : ITenantProvisioner
{
    private readonly ITenantDatabaseRepository _tenantDatabaseRepository;
    private readonly IDatabaseServerRepository _databaseServerRepository;
    private readonly IDatabaseCredentialRepository _databaseCredentialRepository;
    private readonly ILogger<SqlServerTenantProvisioner> _logger;

    public SqlServerTenantProvisioner(
        ITenantDatabaseRepository tenantDatabaseRepository,
        IDatabaseServerRepository databaseServerRepository,
        IDatabaseCredentialRepository databaseCredentialRepository,
        ILogger<SqlServerTenantProvisioner> logger)
    {
        _tenantDatabaseRepository = tenantDatabaseRepository;
        _databaseServerRepository = databaseServerRepository;
        _databaseCredentialRepository = databaseCredentialRepository;
        _logger = logger;
    }

    public async Task ProvisionAsync(
        Guid tenantDatabaseId,
        CancellationToken cancellationToken = default)
    {
        var tenantDatabase =
            await _tenantDatabaseRepository.GetByIdAsync(
                tenantDatabaseId,
                cancellationToken);

        if (tenantDatabase is null)
        {
            throw new InvalidOperationException(
                $"TenantDatabase '{tenantDatabaseId}' no existe.");
        }

        var databaseServer =
            await _databaseServerRepository.GetByIdAsync(
                tenantDatabase.DatabaseServerId,
                cancellationToken);

        if (databaseServer is null)
        {
            throw new InvalidOperationException(
                $"DatabaseServer '{tenantDatabase.DatabaseServerId}' no existe.");
        }

        if (!databaseServer.IsActive)
        {
            throw new InvalidOperationException(
                $"DatabaseServer '{databaseServer.Code}' está inactivo.");
        }

        var credential =
            await _databaseCredentialRepository
                .GetByDatabaseServerIdAsync(
                    databaseServer.Id,
                    cancellationToken);

        if (credential is null)
        {
            throw new InvalidOperationException(
                $"No existe una credencial activa para el servidor '{databaseServer.Code}'.");
        }

        var databaseName =
            tenantDatabase.PhysicalDatabaseName;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException(
                $"TenantDatabase '{tenantDatabaseId}' no tiene PhysicalDatabaseName.");
        }

        _logger.LogInformation(
            "Provisionando TenantDatabase {DatabaseName} en {Server}.",
            databaseName,
            databaseServer.Host);

        var connectionBuilder =
            new SqlConnectionStringBuilder
            {
                DataSource =
                    $"{databaseServer.Host},{databaseServer.Port}",

                InitialCatalog =
                    databaseName,

                UserID =
                    credential.Username,

                Password =
                    credential.PasswordSecret,

                TrustServerCertificate = true,

                Encrypt = true
            };

        var tenantConnection =
            connectionBuilder.ConnectionString;

        var options =
            new DbContextOptionsBuilder<TenantDbContext>()
                .UseSqlServer(
                    tenantConnection,
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(
                            typeof(TenantDbContext).Assembly.FullName);
                    })
                .Options;

        await using var context =
            new TenantDbContext(options);

        _logger.LogInformation(
            "Validando conexión a {DatabaseName}.",
            databaseName);

        await context.Database.OpenConnectionAsync(
            cancellationToken);

        await context.Database.CloseConnectionAsync();

        _logger.LogInformation(
            "Conexión a {DatabaseName} validada.",
            databaseName);

        await context.Database.MigrateAsync(
            cancellationToken);

        _logger.LogInformation(
            "Migraciones Tenant aplicadas a {DatabaseName}.",
            databaseName);

        tenantDatabase.MarkReady();

        await _tenantDatabaseRepository.UpdateAsync(
            tenantDatabase,
            cancellationToken);

        _logger.LogInformation(
            "TenantDatabase {DatabaseId} quedó en Ready.",
            tenantDatabase.Id);
    }
}
