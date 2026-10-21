using Loyaltybenefit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

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
                // 1. Validar Hotel
                var tenant = await platformDb.Tenants.FindAsync(request.TenantId);
                if (tenant == null) return Results.BadRequest("Hotel no encontrado.");

                // 2. Obtener la BD del hotel
                var tenantDbInfo = await platformDb.TenantDatabases
                    .FirstOrDefaultAsync(td => td.TenantId == request.TenantId && td.IsActive);
                
                if (tenantDbInfo == null) return Results.BadRequest("El hotel no tiene BD asignada.");

                // 3. Calcular puntos
                var points = (int)(request.Amount / 100);
                if (points <= 0) return Results.BadRequest("El monto es muy bajo.");

                // 4. Conectar a la BD del VPS y guardar
                var connectionString = $"Server=contabo.ciberweb.com.mx,1433;Database={tenantDbInfo.PhysicalDatabaseName};User Id=sa;Password=Aguador#3985;TrustServerCertificate=True;";
                
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // --- CREAR HUÉSPED SI NO EXISTE ---
                    var checkGuest = "SELECT COUNT(1) FROM Guests WHERE Id = @GuestId";
                    using (var cmd = new SqlCommand(checkGuest, connection))
                    {
                        cmd.Parameters.AddWithValue("@GuestId", request.GuestId);
                        var exists = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                        
                        if (exists == 0)
                        {
                            var insertGuest = "INSERT INTO Guests (Id, Name, Email, IsActive, CreatedAtUtc) VALUES (@Id, 'Cliente VPS', 'test@vps.com', 1, GETUTCDATE())";
                            using (var insertGuestCmd = new SqlCommand(insertGuest, connection))
                            {
                                insertGuestCmd.Parameters.AddWithValue("@Id", request.GuestId);
                                await insertGuestCmd.ExecuteNonQueryAsync();
                            }
                        }
                    }

                    // --- CREAR CUENTA DE LEALTAD SI NO EXISTE ---
                    var checkAccount = "SELECT COUNT(1) FROM LoyaltyAccounts WHERE GuestId = @GuestId";
                    using (var cmd = new SqlCommand(checkAccount, connection))
                    {
                        cmd.Parameters.AddWithValue("@GuestId", request.GuestId);
                        var exists = (int)(await cmd.ExecuteScalarAsync() ?? 0);

                        if (exists == 0)
                        {
                            var insertAccount = "INSERT INTO LoyaltyAccounts (Id, GuestId, PointsBalance, MonetaryBalance, IsActive, CreatedAtUtc) VALUES (@Id, @GuestId, 0, 0, 1, GETUTCDATE())";
                            using (var insertAccountCmd = new SqlCommand(insertAccount, connection))
                            {
                                insertAccountCmd.Parameters.AddWithValue("@Id", Guid.NewGuid());
                                insertAccountCmd.Parameters.AddWithValue("@GuestId", request.GuestId);
                                await insertAccountCmd.ExecuteNonQueryAsync();
                            }
                        }
                    }

                    // --- SUMAR PUNTOS ---
                    var updatePoints = "UPDATE LoyaltyAccounts SET PointsBalance = PointsBalance + @Points WHERE GuestId = @GuestId";
                    using (var updateCmd = new SqlCommand(updatePoints, connection))
                    {
                        updateCmd.Parameters.AddWithValue("@Points", points);
                        updateCmd.Parameters.AddWithValue("@GuestId", request.GuestId);
                        await updateCmd.ExecuteNonQueryAsync();
                    }

                    // --- OBTENER SALDO FINAL ---
                    var getBalance = "SELECT PointsBalance FROM LoyaltyAccounts WHERE GuestId = @GuestId";
                    int newBalance = 0;
                    using (var cmd = new SqlCommand(getBalance, connection))
                    {
                        cmd.Parameters.AddWithValue("@GuestId", request.GuestId);
                        newBalance = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                    }

                    return Results.Ok(new { 
                        Mensaje = $"Puntos guardados en el VPS para {tenant.Name}",
                        PuntosGanados = points,
                        SaldoActual = newBalance
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR EN VPS: {ex.Message}");
                return Results.BadRequest($"Error de SQL: {ex.Message}");
            }
        });
    }
}

public record CreateStayRequest(Guid TenantId, Guid GuestId, Guid StayId, decimal Amount);
