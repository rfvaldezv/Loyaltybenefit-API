using Loyaltybenefit.Application.Tenants;
using Loyaltybenefit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Loyaltybenefit.Api.Endpoints;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tenants")
            .WithTags("Tenants")
            .RequireAuthorization();

        // 👇 1. ENDPOINT PARA LISTAR (GET)
        group.MapGet("/", async (PlatformDbContext db) =>
        {
            var tenants = await db.Tenants
                .Select(t => new { t.Id, t.Name, t.Code, t.CreatedAtUtc })
                .ToListAsync();
            
            return Results.Ok(tenants);
        });

        // 👇 2. ENDPOINT PARA VER UNO SOLO (GET POR ID)
        group.MapGet("/{id:guid}", async (Guid id, PlatformDbContext db) =>
        {
            var tenant = await db.Tenants
                .Where(t => t.Id == id)
                .Select(t => new { t.Id, t.Name, t.Code, t.CreatedAtUtc })
                .FirstOrDefaultAsync();

            if (tenant == null) return Results.NotFound("Hotel no encontrado.");

            return Results.Ok(tenant);
        });

        // 👇 3. ENDPOINT PARA CREAR (POST)
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
