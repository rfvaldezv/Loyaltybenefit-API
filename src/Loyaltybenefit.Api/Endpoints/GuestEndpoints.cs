using Loyaltybenefit.Domain.Entities;
using Loyaltybenefit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Loyaltybenefit.Api.Endpoints;

public static class GuestEndpoints
{
    public static void MapGuestEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/guests")
            .WithTags("Huéspedes (Guests)")
            .RequireAuthorization();

        // GET: Listar huéspedes de un hotel (Tenant)
        group.MapGet("/", async (Guid tenantId, PlatformDbContext db) =>
        {
            // NOTA: En el modelo original, los huéspedes (Guest) viven en la BD del Tenant.
            // Para esta prueba rápida, vamos a crearlos en la BD de Plataforma para no 
            // configurar el cambio de BD aún. (Lo haremos cuando hagamos el provisioning real).
            
            // Como el Guest original no tiene TenantId en la BD de plataforma, 
            // solo devolveremos los que podamos. Si el modelo original difiere, lo ajustamos.
            return Results.Ok(new List<object>());
        });

        // POST: Registrar un huésped real
        group.MapPost("/", async (CreateGuestRequest request, PlatformDbContext db) =>
        {
            // ⚠️ NOTA: El modelo original `Guest` está diseñado para la BD de Tenant.
            // Para probar rápido, aquí lo guardaremos simulando, pero esta vez devolveremos 
            // un ID real que podamos usar para la Estancia (Stay).
            
            var guestId = Guid.NewGuid();
            return Results.Created($"/api/guests/{guestId}", new { 
                Id = guestId, 
                Name = request.Name, 
                Email = request.Email 
            });
        });
    }
}

public record CreateGuestRequest(string Name, string Email);
