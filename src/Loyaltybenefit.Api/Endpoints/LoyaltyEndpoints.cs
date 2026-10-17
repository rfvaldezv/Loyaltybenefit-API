using Loyaltybenefit.Application.Loyalty;

namespace Loyaltybenefit.Api.Endpoints;

public static class LoyaltyEndpoints
{
    public static void MapLoyaltyEndpoints(
        this WebApplication app)
    {
        app.MapPost(
            "/api/loyalty/earn",
            async (
                EarnPointsRequest request,
                EarnPointsService service,
                CancellationToken cancellationToken) =>
            {
                var command = new EarnPointsCommand(
                    request.TenantId,
                    request.GuestId,
                    request.StayId);

                var result =
                    await service.ExecuteAsync(
                        command,
                        cancellationToken);

                return Results.Ok(result);
            });

        app.MapPost(
            "/api/loyalty/redeem",
            async (
                RedeemPointsRequest request,
                RedeemPointsService service,
                CancellationToken cancellationToken) =>
            {
                var command = new RedeemPointsCommand(
                    request.TenantId,
                    request.GuestId,
                    request.Points,
                    request.Reference);

                var result =
                    await service.ExecuteAsync(
                        command,
                        cancellationToken);

                return Results.Ok(result);
            });
    }
}

public sealed record EarnPointsRequest(
    Guid TenantId,
    Guid GuestId,
    Guid StayId);

public sealed record RedeemPointsRequest(
    Guid TenantId,
    Guid GuestId,
    int Points,
    string Reference);
