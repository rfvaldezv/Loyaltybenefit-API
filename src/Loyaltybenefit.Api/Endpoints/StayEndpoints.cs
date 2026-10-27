using Loyaltybenefit.Application.Loyalty;
using Loyaltybenefit.Domain.Entities;
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
            PlatformDbContext platformDb,
            ITenantContext tenantContext,
            TenantDbContextFactory tenantDbFactory) =>
        {
            try
            {
                // 1. Validar que el hotel existe
                var tenant = await platformDb.Tenants.FindAsync(request.TenantId);
                if (tenant == null) return Results.BadRequest("Hotel no encontrado.");

                // 2. Decirle al sistema a qué base de datos conectarse (seteando el TenantContext)
                tenantContext.SetTenant(request.TenantId);

                // 3. La fábrica crea el contexto para la BD del hotel
                var tenantDb = await tenantDbFactory.CreateAsync();

                // 4. Buscar al Huésped. Si no existe, lo creamos.
                var guest = await tenantDb.Guests.FindAsync(request.GuestId);
                if (guest == null)
                {
                    guest = new Guest(request.GuestId, "Cliente Auto", "auto@vps.com");
                    tenantDb.Guests.Add(guest);
                    await tenantDb.SaveChangesAsync();
                }

                // 5. Buscar o crear la Cuenta de Lealtad
                var account = await tenantDb.LoyaltyAccounts
                    .FirstOrDefaultAsync(a => a.GuestId == request.GuestId);

                if (account == null)
                {
                    account = new LoyaltyAccount(Guid.NewGuid(), request.GuestId);
                    tenantDb.LoyaltyAccounts.Add(account);
                }

                // 6. Calcular puntos (usando la regla que ya guardamos)
                var rule = await tenantDb.LoyaltyRules.FirstOrDefaultAsync(r => r.IsActive);
                decimal threshold = rule?.AmountThreshold ?? 100;
                int pointsPerThreshold = rule?.Points ?? 1;

                var points = (int)((request.Amount / threshold) * pointsPerThreshold);
                if (points <= 0) return Results.BadRequest("El monto es muy bajo para generar puntos.");

                // 7. Sumar puntos y registrar la transacción
                account.AddPoints(points);
                var transaction = new PointTransaction(
                    Guid.NewGuid(),
                    account.Id,
                    points,
                    "STAY_EARN",
                    $"Consumo en {tenant.Name}",
                    $"TRX-{DateTime.UtcNow.Ticks}",
                    request.StayId
                );

                tenantDb.PointTransactions.Add(transaction);
                
                // 8. GUARDAR EN LA BD DEL HOTEL
                await tenantDb.SaveChangesAsync();

                return Results.Ok(new { 
                    Mensaje = $"Puntos guardados para {tenant.Name}",
                    PuntosGanados = points,
                    SaldoActual = account.PointsBalance
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
