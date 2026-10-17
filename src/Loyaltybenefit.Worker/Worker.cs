using Loyaltybenefit.Application.Provisioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Loyaltybenefit.Worker;

public sealed class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IServiceScopeFactory scopeFactory,
        ILogger<Worker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Loyaltybenefit Worker iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var repository =
                    scope.ServiceProvider
                        .GetRequiredService<
                            ITenantDatabaseRepository>();

                var provisioner =
                    scope.ServiceProvider
                        .GetRequiredService<
                            ITenantProvisioner>();

                var pending =
                    await repository
                        .GetPendingAsync(stoppingToken);

                foreach (var tenantDatabase in pending)
                {
                    try
                    {
                        tenantDatabase.MarkProvisioning();

                        await repository.UpdateAsync(
                            tenantDatabase,
                            stoppingToken);

                        _logger.LogInformation(
                            "TenantDatabase {DatabaseId} pasó a Provisioning.",
                            tenantDatabase.Id);

                        await provisioner.ProvisionAsync(
                            tenantDatabase.Id,
                            stoppingToken);

                        _logger.LogInformation(
                            "Provisioning completado para {DatabaseId}.",
                            tenantDatabase.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error procesando TenantDatabase {DatabaseId}.",
                            tenantDatabase.Id);

                        tenantDatabase.MarkFailed();

                        await repository.UpdateAsync(
                            tenantDatabase,
                            stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error general del ciclo de provisioning.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(30),
                stoppingToken);
        }
    }
}
