using Loyaltybenefit.Application.Loyalty;
using Loyaltybenefit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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
                // 1. Validar que el Hotel exista
                var tenant = await platformDb.Tenants.FindAsync(request.TenantId);
                if (tenant == null) return Results.BadRequest("Hotel no encontrado.");

                // 2. Calcular puntos (1 punto por cada $100)
                var points = (int)(request.Amount / 100);
                if (points <= 0) return Results.BadRequest("El monto es muy bajo para generar puntos.");

                // 3. Simulamos el guardado en la BD del Hotel (para ver la app funcionar YA)
                var saldoSimulado = points; 

                return Results.Ok(new { 
                    Mensaje = $"Puntos guardados exitosamente en {tenant.Name}",
                    PuntosGanados = points,
                    SaldoActual = saldoSimulado,
                    BaseDeDatos = "Simulada (Lista para conectar)"
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
