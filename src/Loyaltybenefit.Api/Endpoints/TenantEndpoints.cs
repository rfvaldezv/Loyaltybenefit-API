using Loyaltybenefit.Application.Tenants;

namespace Loyaltybenefit.Api.Endpoints;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(
        this WebApplication app)
    {
        app.MapPost(
            "/api/tenants",
            async (
                CreateTenantRequest request,
                CreateTenantService service,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateTenantCommand(
                    request.Code,
                    request.Name,
                    request.IndustryCode);

                var result =
                    await service.ExecuteAsync(
                        command,
                        cancellationToken);

                return Results.Created(
                    $"/api/tenants/{result.Id}",
                    result);
            });
    }
}

public sealed record CreateTenantRequest(
    string Code,
    string Name,
    string IndustryCode);
    