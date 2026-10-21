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

        // POST: Registrar una estancia (Simulación de cálculo de puntos)
        group.MapPost("/", async (
            CreateStayRequest request, 
            PlatformDbContext db) =>
        {
            try
            {
                // 1. Validar que el Hotel exista (esto sí está en la BD de plataforma)
                var tenant = await db.Tenants.FindAsync(request.TenantId);
                if (tenant == null) return Results.BadRequest("Hotel no encontrado.");

                // 2. Calcular puntos (Regla de prueba: 1 punto por cada $100)
                var points = (int)(request.Amount / 100);
                if (points <= 0) return Results.BadRequest("El monto es muy bajo para generar puntos.");

                // 3. Simulamos el saldo (en la siguiente sesión conectaremos la BD real del Hotel)
                // Por ahora, devolvemos éxito para que Flutter funcione.
                var saldoSimulado = points; 

                return Results.Ok(new { 
                    Mensaje = "Puntos calculados exitosamente",
                    PuntosGanados = points,
                    SaldoActual = saldoSimulado 
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest($"Error de negocio: {ex.Message}");
            }
        });
    }
}

public record CreateStayRequest(Guid TenantId, Guid GuestId, decimal Amount);
