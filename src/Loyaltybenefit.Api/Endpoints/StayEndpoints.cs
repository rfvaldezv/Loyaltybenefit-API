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
                var tenant = await platformDb.Tenants.FindAsync(request.TenantId);
                if (tenant == null) return Results.BadRequest("Hotel no encontrado.");

                // 1. Calcular puntos (Regla: 1 punto por cada $100)
                var points = (int)(request.Amount / 100);
                if (points <= 0) return Results.BadRequest("El monto es muy bajo para generar puntos.");

                // 2. Devolver respuesta de éxito (simulación de guardado multi-tenant)
                return Results.Ok(new { 
                    Mensaje = $"Puntos calculados exitosamente para {tenant.Name}",
                    PuntosGanados = points,
                    SaldoActual = points // Saldo simulado
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
