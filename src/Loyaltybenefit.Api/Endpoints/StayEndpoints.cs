using Loyaltybenefit.Infrastructure.Persistence;

namespace Loyaltybenefit.Api.Endpoints;

public static class StayEndpoints
{
    public static void MapStayEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/stays")
            .WithTags("Estancias (Stays)")
            .RequireAuthorization();

        group.MapPost("/", async (
            CreateStayRequest request, 
            PlatformDbContext platformDb) =>
        {
            try
            {
                // 1. Validar Hotel
                var tenant = await platformDb.Tenants.FindAsync(request.TenantId);
                if (tenant == null) return Results.BadRequest("Hotel no encontrado.");

                // 2. Calcular puntos (Regla: 1 punto por cada $100)
                var points = (int)(request.Amount / 100);
                if (points <= 0) return Results.BadRequest("El monto es muy bajo para generar puntos.");

                // 3. Simulamos el guardado (la persistencia real se hará en la BD del Tenant)
                // Por ahora devolvemos el éxito para que la UI funcione.
                var saldoSimulado = points; 

                return Results.Ok(new { 
                    Mensaje = $"Puntos calculados exitosamente para {tenant.Name}",
                    PuntosGanados = points,
                    SaldoActual = saldoSimulado
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest($"Error: {ex.Message}");
            }
        });
    }
}

public record CreateStayRequest(Guid TenantId, Guid GuestId, Guid StayId, decimal Amount);
