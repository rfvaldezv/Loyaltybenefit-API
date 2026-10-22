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
                // 1. Validar Hotel y obtener BD
                var tenant = await platformDb.Tenants.FindAsync(request.TenantId);
                if (tenant == null) return Results.BadRequest("Hotel no encontrado.");

                var tenantDbInfo = await platformDb.TenantDatabases
                    .FirstOrDefaultAsync(td => td.TenantId == request.TenantId && td.IsActive);
                
                if (tenantDbInfo == null) return Results.BadRequest("El hotel no tiene BD asignada.");

                // 2. Conectar a la BD del VPS
                var connectionString = $"Server=VMI1203198\\MSSQLSERVER2019;Database={tenantDbInfo.PhysicalDatabaseName};User Id=sa;Password=Aguador#3985;TrustServerCertificate=True;";
                
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // 3. CONSULTAR LA REGLA DE LEALTAD
                    decimal threshold = 100; // Valor por defecto si no hay regla
                    int pointsPerThreshold = 1; // Valor por defecto

                    var ruleQuery = "SELECT TOP 1 AmountThreshold, Points FROM LoyaltyRules WHERE IsActive = 1";
                    using (var cmd = new SqlCommand(ruleQuery, connection))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                threshold = reader.GetDecimal(0);
                                pointsPerThreshold = reader.GetInt32(1);
                            }
                        }
                    }

                    // 4. Calcular puntos basados en la regla
                    var points = (int)((request.Amount / threshold) * pointsPerThreshold);
                    if (points <= 0) return Results.BadRequest($"El monto (${request.Amount}) no genera puntos con la regla actual (${threshold} = {pointsPerThreshold} pts).");

                    // 5. Crear Huésped si no existe
                    var checkGuest = "SELECT COUNT(1) FROM Guests WHERE Id = @GuestId";
                    using (var cmd = new SqlCommand(checkGuest, connection))
                    {
                        cmd.Parameters.AddWithValue("@GuestId", request.GuestId);
                        if ((int)(await cmd.ExecuteScalarAsync() ?? 0) == 0)
                        {
                            var insertGuest = "INSERT INTO Guests (Id, Name, Email, IsActive, CreatedAtUtc) VALUES (@Id, 'Cliente VPS', 'test@vps.com', 1, GETUTCDATE())";
                            using (var insertGuestCmd = new SqlCommand(insertGuest, connection))
                            {
                                insertGuestCmd.Parameters.AddWithValue("@Id", request.GuestId);
                                await insertGuestCmd.ExecuteNonQueryAsync();
                            }
                        }
                    }

                    // 6. Crear Cuenta de Lealtad si no existe
                    var checkAccount = "SELECT COUNT(1) FROM LoyaltyAccounts WHERE GuestId = @GuestId";
                    using (var cmd = new SqlCommand(checkAccount, connection))
                    {
                        cmd.Parameters.AddWithValue("@GuestId", request.GuestId);
                        if ((int)(await cmd.ExecuteScalarAsync() ?? 0) == 0)
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

                    // 7. Sumar Puntos
                    var updatePoints = "UPDATE LoyaltyAccounts SET PointsBalance = PointsBalance + @Points WHERE GuestId = @GuestId";
                    using (var updateCmd = new SqlCommand(updatePoints, connection))
                    {
                        updateCmd.Parameters.AddWithValue("@Points", points);
                        updateCmd.Parameters.AddWithValue("@GuestId", request.GuestId);
                        await updateCmd.ExecuteNonQueryAsync();
                    }

                    // 8. Registrar la Transacción
                    var insertTransaction = "INSERT INTO PointTransactions (Id, LoyaltyAccountId, Points, TransactionType, Description, Reference, StayId, CreatedAtUtc) VALUES (@Id, (SELECT Id FROM LoyaltyAccounts WHERE GuestId = @GuestId), @Points, 'STAY_EARN', @Description, @Reference, @StayId, GETUTCDATE())";
                    using (var trxCmd = new SqlCommand(insertTransaction, connection))
                    {
                        trxCmd.Parameters.AddWithValue("@Id", Guid.NewGuid());
                        trxCmd.Parameters.AddWithValue("@GuestId", request.GuestId);
                        trxCmd.Parameters.AddWithValue("@Points", points);
                        trxCmd.Parameters.AddWithValue("@Description", $"Consumo en {tenant.Name}");
                        trxCmd.Parameters.AddWithValue("@Reference", $"TRX-{DateTime.UtcNow.Ticks}");
                        trxCmd.Parameters.AddWithValue("@StayId", request.StayId);
                        await trxCmd.ExecuteNonQueryAsync();
                    }

                    // 9. Obtener Saldo Final
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
                        SaldoActual = newBalance,
                        ReglaAplicada = $"${threshold} = {pointsPerThreshold} pts"
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
