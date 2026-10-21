using Loyaltybenefit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Loyaltybenefit.Api.Endpoints;

public static class GuestPortalEndpoints
{
    public static void MapGuestPortalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/portal")
            .WithTags("Portal del Huésped");

        // GET: Consultar saldo por correo (Endpoint público para huéspedes)
        group.MapGet("/balance", async (
            string email, 
            PlatformDbContext db) =>
        {
            // ⚠️ SIMULACIÓN: Como no hemos guardado los huéspedes reales en la BD,
            // vamos a devolver un saldo simulado basado en el correo.
            // En la siguiente sesión, buscaremos al Guest en la BD y luego sus puntos en la BD del Hotel.

            if (string.IsNullOrWhiteSpace(email)) 
                return Results.BadRequest("El correo es requerido.");

            // Simulamos que el cliente tiene 15 puntos
            var saldoSimulado = 15; 

            return Results.Ok(new { 
                Email = email,
                Puntos = saldoSimulado,
                Mensaje = "¡Bienvenido! Este es tu saldo actual."
            });
        });
    }
}
