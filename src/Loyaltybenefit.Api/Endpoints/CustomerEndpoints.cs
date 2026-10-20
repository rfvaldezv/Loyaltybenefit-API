using Loyaltybenefit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Loyaltybenefit.Api.Endpoints;

public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/customers")
            .WithTags("Clientes (Huéspedes)")
            .RequireAuthorization();

        // GET: Listar clientes
        group.MapGet("/", async (PlatformDbContext db) =>
        {
            // NOTA: Estamos simulando la tabla, ya que los clientes reales viven en la BD de cada Tenant.
            // Para esta prueba, devolveremos una lista vacía o los datos si tuviéramos la tabla.
            return Results.Ok(new List<object>()); 
        });

        // POST: Registrar un cliente (Simulación en memoria por ahora)
        group.MapPost("/", (CreateCustomerRequest request) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
                return Results.BadRequest("El nombre y el correo son obligatorios.");

            // Simulamos la creación del cliente
            var nuevoCliente = new
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow
            };

            return Results.Created($"/api/customers/{nuevoCliente.Id}", nuevoCliente);
        });
    }
}

public record CreateCustomerRequest(
    [Required] string Name, 
    [Required] string Email
);
