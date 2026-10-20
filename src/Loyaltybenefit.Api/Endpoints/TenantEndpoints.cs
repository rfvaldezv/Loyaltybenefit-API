using Loyaltybenefit.Application.Tenants;
using Loyaltybenefit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Loyaltybenefit.Api.Endpoints;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tenants")
            .WithTags("Tenants")
            .RequireAuthorization();

        // 👇 NUEVO: Endpoint para listar los hoteles (GET)
        group.MapGet("/", async (PlatformDbContext db) =>
        {
            var tenants = await db.Tenants
                .Select(t => new { t.Id, t.Name, t.Code, t.CreatedAtUtc })
                .ToListAsync();
            
            return Results.Ok(tenants);
        });

        // Endpoint original para crear (POST)
        group.MapPost("/", async (
            CreateTenantRequest request,
            CreateTenantService service,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateTenantCommand(
                request.Code,
                request.Name,
                request.IndustryCode);

            var result = await service.ExecuteAsync(command, cancellationToken);

            return Results.Created($"/api/tenants/{result.Id}", result);
        });
    }
}

public sealed record CreateTenantRequest(
    string Code,
    string Name,
    string IndustryCode);
