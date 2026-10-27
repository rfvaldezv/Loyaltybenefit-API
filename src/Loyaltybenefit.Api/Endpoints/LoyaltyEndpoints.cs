using Loyaltybenefit.Application.Loyalty;
using Loyaltybenefit.Domain.Entities;
using Loyaltybenefit.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Loyaltybenefit.Api.Endpoints;

public static class LoyaltyEndpoints
{
    public static void MapLoyaltyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/loyalty")
            .WithTags("Lealtad (Puntos)")
            .RequireAuthorization();

        // POST: Sumar puntos
        group.MapPost("/earn", async (
            EarnPointsApiRequest request,
            TenantDbContextFactory dbFactory,
            ITenantContext tenantContext,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            if (tenantContext.TenantId == Guid.Empty)
            {
                if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader) && 
                    Guid.TryParse(tenantHeader, out var parsedTenantId))
                {
                    tenantContext.SetTenant(parsedTenantId);
                }
                else
                {
                    return Results.BadRequest("Falta el TenantId.");
                }
            }

            using var db = await dbFactory.CreateAsync(ct);

            var rule = await db.LoyaltyRules.FirstOrDefaultAsync(r => r.IsActive, ct);
            if (rule == null) return Results.BadRequest("No hay reglas de puntos activas.");

            int points = (int)(Math.Floor(request.Amount / rule.AmountThreshold) * rule.Points);
            if (points <= 0) return Results.BadRequest("El monto no genera puntos.");

            var account = await db.LoyaltyAccounts.FirstOrDefaultAsync(a => a.GuestId == request.GuestId, ct);
            
            if (account == null)
            {
                account = new LoyaltyAccount(Guid.NewGuid(), request.GuestId);
                account.AddPoints(points);
                
                db.LoyaltyAccounts.Add(account);
            }
            else
            {
                account.AddPoints(points);
            }

            await db.SaveChangesAsync(ct);

            return Results.Ok(new 
            { 
                mensaje = "Puntos sumados correctamente", 
                puntosGanados = points, 
                saldoTotal = account.PointsBalance 
            });
        });
    }
}

public record EarnPointsApiRequest(
    Guid GuestId,
    decimal Amount
);
