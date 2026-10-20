using Loyaltybenefit.Application.Loyalty;
using Microsoft.AspNetCore.Authorization;

namespace Loyaltybenefit.Api.Endpoints;

public static class LoyaltyEndpoints
{
    public static void MapLoyaltyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/loyalty")
            .WithTags("Lealtad (Puntos)")
            .RequireAuthorization(); // 🔒 Requiere JWT

        // POST: Sumar puntos (Basado en Estancia/Stay)
        group.MapPost("/earn", async (
            EarnPointsCommand command,
            EarnPointsService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.ExecuteAsync(command, cancellationToken);
            return Results.Ok(result);
        });

        // POST: Canjear puntos
        group.MapPost("/redeem", async (
            RedeemPointsCommand command,
            RedeemPointsService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.ExecuteAsync(command, cancellationToken);
            return Results.Ok(result);
        });
    }
}
