using Loyaltybenefit.Application.Provisioning;

namespace Loyaltybenefit.Api.Endpoints;

public static class TenantDatabaseEndpoints
{
    public static void MapTenantDatabaseEndpoints(
        this WebApplication app)
    {
        app.MapPost(
            "/api/tenants/{tenantId:guid}/database",
            async (
                Guid tenantId,
                CreateTenantDatabaseService service,
                CancellationToken cancellationToken) =>
            {
                var databaseId =
                    await service.ExecuteAsync(
                        tenantId,
                        cancellationToken);

                return Results.Ok(new
                {
                    tenantId,
                    databaseId
                });
            });
    }
}
