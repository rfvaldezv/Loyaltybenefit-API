using Loyaltybenefit.Infrastructure.Persistence;
using Loyaltybenefit.Application.Provisioning;

namespace Loyaltybenefit.Api.Endpoints;

public static class ProvisioningEndpoints
{
    public static void MapProvisioningEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/provisioning")
            .WithTags("Provisioning (Creación de BDs)")
            .RequireAuthorization();

        group.MapPost("/{tenantId:guid}", async (
            Guid tenantId, 
            PlatformDbContext db) =>
        {
            var tenant = await db.Tenants.FindAsync(tenantId);
            if (tenant == null) return Results.NotFound("Hotel no encontrado.");

            // En la siguiente sesión, conectaremos el provisionador real (ITenantProvisioner)
            // Por ahora, devolvemos éxito para no romper el flujo.
            return Results.Ok(new { 
                Mensaje = "✅ Base de datos del hotel creada exitosamente (simulación)",
                Hotel = tenant.Name
            });
        });
    }
}
