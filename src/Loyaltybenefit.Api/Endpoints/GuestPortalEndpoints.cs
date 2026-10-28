using Loyaltybenefit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Loyaltybenefit.Api.Endpoints;

public static class GuestPortalEndpoints
{
    public static void MapGuestPortalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/portal")
            .WithTags("Portal del Huésped");

        // GET: Consultar saldo
        group.MapGet("/balance", async (
            string email, string tenantId, PlatformDbContext platformDb, IConfiguration config) =>
        {
            if (string.IsNullOrWhiteSpace(email)) return Results.BadRequest("El correo es requerido.");
            try
            {
                var connectionString = string.Format(config.GetConnectionString("TenantTemplate")!, "CONCEPT_HOTEL_DB");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = "SELECT g.FirstName, g.Email, a.PointsBalance FROM Guests g LEFT JOIN LoyaltyAccounts a ON g.Id = a.GuestId WHERE g.Email = @Email";
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Email", email);
                using var reader = await cmd.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    return Results.Ok(new
                    {
                        Cliente = reader.GetString(0),
                        Email = reader.GetString(1),
                        Puntos = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                        Nivel = (reader.IsDBNull(2) ? 0 : reader.GetInt32(2)) > 500 ? "VIP Gold" : "Miembro",
                        Mensaje = "¡Bienvenido!"
                    });
                }
                return Results.NotFound(new { Mensaje = "No encontramos un cliente con ese correo." });
            }
            catch (Exception ex) { return Results.BadRequest($"Error: {ex.Message}"); }
        });

        // GET: Historial de movimientos (Statement / Timeline)
        group.MapGet("/statement", async (
            string email, string tenantId, PlatformDbContext platformDb, IConfiguration config) =>
        {
            if (string.IsNullOrWhiteSpace(email)) return Results.BadRequest("El correo es requerido.");
            try
            {
                var connectionString = string.Format(config.GetConnectionString("TenantTemplate")!, "CONCEPT_HOTEL_DB");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT pt.Id, pt.Points, pt.Type, pt.Description, pt.Reference, pt.CreatedAtUtc
                    FROM PointTransactions pt
                    INNER JOIN LoyaltyAccounts a ON pt.LoyaltyAccountId = a.Id
                    INNER JOIN Guests g ON a.GuestId = g.Id
                    WHERE g.Email = @Email
                    ORDER BY pt.CreatedAtUtc DESC";

                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Email", email);
                
                var transactions = new List<object>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    transactions.Add(new
                    {
                        Id = reader.GetGuid(0),
                        Points = reader.GetInt32(1),
                        Type = reader.GetString(2),
                        Description = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        Reference = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        Date = reader.GetDateTime(5)
                    });
                }
                return Results.Ok(new { Email = email, TotalMovimientos = transactions.Count, Movimientos = transactions });
            }
            catch (Exception ex) { return Results.BadRequest($"Error: {ex.Message}"); }
        });

        // GET: Catálogo de Premios
        group.MapGet("/rewards", async (IConfiguration config) =>
        {
            try
            {
                var connectionString = string.Format(config.GetConnectionString("TenantTemplate")!, "CONCEPT_HOTEL_DB");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = "SELECT Id, Name, Description, PointsCost, IsPhysical, ImageUrl FROM Rewards WHERE IsActive = 1";
                using var cmd = new SqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var rewards = new List<object>();
                while (await reader.ReadAsync())
                {
                    rewards.Add(new
                    {
                        Id = reader.GetGuid(0),
                        Name = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        PointsCost = reader.GetInt32(3),
                        IsPhysical = reader.GetBoolean(4),
                        ImageUrl = reader.IsDBNull(5) ? "" : reader.GetString(5)
                    });
                }
                return Results.Ok(rewards);
            }
            catch (Exception ex) { return Results.BadRequest($"Error: {ex.Message}"); }
        });

        // POST: Redimir un premio (Actualizado para envíos)
        group.MapPost("/redeem", async (
            RedeemRewardRequest request, IConfiguration config) =>
        {
            try
            {
                var connectionString = string.Format(config.GetConnectionString("TenantTemplate")!, "CONCEPT_HOTEL_DB");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // 1. Validar saldo y obtener cuenta
                var query = "SELECT a.Id, a.PointsBalance, g.Id FROM Guests g JOIN LoyaltyAccounts a ON g.Id = a.GuestId WHERE g.Email = @Email";
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Email", request.GuestEmail);
                
                Guid accountId = Guid.Empty;
                Guid guestId = Guid.Empty;
                int currentPoints = 0;

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        accountId = reader.GetGuid(0);
                        currentPoints = reader.GetInt32(1);
                        guestId = reader.GetGuid(2);
                    }
                    else return Results.NotFound("Cliente no encontrado.");
                }

                if (currentPoints < request.PointsToRedeem)
                    return Results.BadRequest($"Puntos insuficientes. Tienes {currentPoints}.");

                // 2. Descontar puntos
                var updateQuery = "UPDATE LoyaltyAccounts SET PointsBalance = PointsBalance - @Points WHERE Id = @Id";
                using (var updateCmd = new SqlCommand(updateQuery, connection))
                {
                    updateCmd.Parameters.AddWithValue("@Points", request.PointsToRedeem);
                    updateCmd.Parameters.AddWithValue("@Id", accountId);
                    await updateCmd.ExecuteNonQueryAsync();
                }

                // 3. Guardar en historial
                var insertTx = "INSERT INTO PointTransactions (Id, LoyaltyAccountId, Points, Type, Description, CreatedAtUtc) VALUES (NEWID(), @AccountId, @Points, 'Redeem', @Desc, GETUTCDATE())";
                using (var txCmd = new SqlCommand(insertTx, connection))
                {
                    txCmd.Parameters.AddWithValue("@AccountId", accountId);
                    txCmd.Parameters.AddWithValue("@Points", -request.PointsToRedeem);
                    txCmd.Parameters.AddWithValue("@Desc", $"Canje: {request.RewardName}");
                    await txCmd.ExecuteNonQueryAsync();
                }

                // 4. Si es físico y hay dirección, crear la orden de envío
                if (request.IsPhysical && !string.IsNullOrWhiteSpace(request.ShippingAddress))
                {
                    var insertOrder = @"
                        INSERT INTO RedemptionOrders (GuestId, RewardId, ShippingAddress) 
                        VALUES (@GuestId, @RewardId, @Address)";
                    using (var orderCmd = new SqlCommand(insertOrder, connection))
                    {
                        orderCmd.Parameters.AddWithValue("@GuestId", guestId);
                        orderCmd.Parameters.AddWithValue("@RewardId", request.RewardId ?? Guid.NewGuid());
                        orderCmd.Parameters.AddWithValue("@Address", request.ShippingAddress);
                        await orderCmd.ExecuteNonQueryAsync();
                    }
                }

                return Results.Ok(new { mensaje = "¡Canje exitoso!", nuevoSaldo = currentPoints - request.PointsToRedeem, item = request.RewardName });
            }
            catch (Exception ex) { return Results.BadRequest($"Error: {ex.Message}"); }
        });

        // NUEVO GET: Branding del Hotel (Colores y Logo)
        // GET: Branding del Hotel
        group.MapGet("/branding", async (
            string tenantId,
            PlatformDbContext platformDb) =>
        {
            if (!Guid.TryParse(tenantId, out var parsedTenantId))
                return Results.BadRequest("TenantId inválido.");

            var tenant = await platformDb.Tenants.FirstOrDefaultAsync(t => t.Id == parsedTenantId);
            
            if (tenant == null) 
                return Results.NotFound("Hotel no encontrado.");

            return Results.Ok(new
            {
                Name = tenant.Name,
                LogoUrl = tenant.LogoUrl ?? "", // Ej: "/logos/guid_123.png"
                PrimaryColor = tenant.PrimaryColorHex ?? "#4F46E5",
                SecondaryColor = tenant.SecondaryColorHex ?? "#8B5CF6"
            });
        });

        // GET: Listar Propiedades (Para el Dropdown del Admin)
        group.MapGet("/properties", async (IConfiguration config) =>
        {
            try
            {
                var connectionString = string.Format(config.GetConnectionString("TenantTemplate")!, "CONCEPT_HOTEL_DB");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = "SELECT Id, Name, Code FROM Properties WHERE IsActive = 1";
                using var cmd = new SqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var properties = new List<object>();
                while (await reader.ReadAsync())
                {
                    properties.Add(new
                    {
                        Id = reader.GetGuid(0),
                        Name = reader.GetString(1),
                        Code = reader.GetString(2)
                    });
                }
                return Results.Ok(properties);
            }
            catch (Exception ex) { return Results.BadRequest($"Error: {ex.Message}"); }
        });
        // POST: Solicitar código OTP
        group.MapPost("/auth/request-otp", async (OtpRequest request, IConfiguration config) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email)) return Results.BadRequest("El correo es requerido.");

            try
            {
                var connectionString = string.Format(config.GetConnectionString("TenantTemplate")!, "CONCEPT_HOTEL_DB");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // 1. Verificar que el huésped exista
                var checkQuery = "SELECT Id FROM Guests WHERE Email = @Email AND IsActive = 1";
                using (var checkCmd = new SqlCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@Email", request.Email);
                    var exists = await checkCmd.ExecuteScalarAsync();
                    if (exists == null) return Results.NotFound("El correo no está registrado.");
                }

                // 2. Generar OTP de 6 dígitos y expiración de 5 minutos
                var random = new Random();
                var otpCode = random.Next(100000, 999999).ToString();
                var expiresAt = DateTime.UtcNow.AddMinutes(5);

                // 3. Guardar en la base de datos
                var updateQuery = "UPDATE Guests SET OtpCode = @Otp, OtpExpiresAt = @Expires WHERE Email = @Email";
                using (var updateCmd = new SqlCommand(updateQuery, connection))
                {
                    updateCmd.Parameters.AddWithValue("@Otp", otpCode);
                    updateCmd.Parameters.AddWithValue("@Expires", expiresAt);
                    updateCmd.Parameters.AddWithValue("@Email", request.Email);
                    await updateCmd.ExecuteNonQueryAsync();
                }

                // Nota: En producción, aquí enviarías un correo con SendGrid/SMTP.
                // Por ahora, devolvemos el OTP en la respuesta para probar.
                return Results.Ok(new { mensaje = "Código enviado", debug_otp = otpCode });
            }
            catch (Exception ex) { return Results.BadRequest($"Error: {ex.Message}"); }
        });

        // POST: Verificar OTP y generar sesión
        group.MapPost("/auth/verify-otp", async (OtpVerifyRequest request, IConfiguration config) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
                return Results.BadRequest("Correo y código son requeridos.");

            try
            {
                var connectionString = string.Format(config.GetConnectionString("TenantTemplate")!, "CONCEPT_HOTEL_DB");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT Id, FirstName, Email 
                    FROM Guests 
                    WHERE Email = @Email 
                      AND OtpCode = @Code 
                      AND OtpExpiresAt > GETUTCDATE()";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Email", request.Email);
                    cmd.Parameters.AddWithValue("@Code", request.Code);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var guestId = reader.GetGuid(0);
                            var firstName = reader.GetString(1);
                            var email = reader.GetString(2);

                            // Limpiar el OTP usado
                            await reader.CloseAsync();
                            using var clearCmd = new SqlCommand("UPDATE Guests SET OtpCode = NULL, OtpExpiresAt = NULL WHERE Id = @Id", connection);
                            clearCmd.Parameters.AddWithValue("@Id", guestId);
                            await clearCmd.ExecuteNonQueryAsync();

                            return Results.Ok(new { mensaje = "Autenticado", guestId = guestId, firstName = firstName, email = email });
                        }
                        return Results.BadRequest("Código inválido o expirado.");
                    }
                }
            }
            catch (Exception ex) { return Results.BadRequest($"Error: {ex.Message}"); }
        });
    }
}

public record RedeemRewardRequest(
    string TenantId,
    string GuestEmail,
    string RewardName,
    int PointsToRedeem,
    bool IsPhysical = false,
    string? ShippingAddress = null,
    Guid? RewardId = null
);

public record OtpRequest(string Email);
public record OtpVerifyRequest(string Email, string Code);
