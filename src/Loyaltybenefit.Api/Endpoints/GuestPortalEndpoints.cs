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

        // GET: Consultar saldo (A PRUEBA DE FALLOS)
        group.MapGet("/balance", async (
            string email, string tenantId, PlatformDbContext platformDb, IConfiguration config) =>
        {
            if (string.IsNullOrWhiteSpace(email)) return Results.BadRequest("El correo es requerido.");
            try
            {
                var connectionString = string.Format(config.GetConnectionString("TenantTemplate")!, "CONCEPT_HOTEL_DB");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // Usamos ISNULL para que devuelva 0 si el cliente no tiene cuenta aún
                var query = @"
                    SELECT g.FirstName, g.Email, ISNULL(a.PointsBalance, 0) as Puntos
                    FROM Guests g 
                    LEFT JOIN LoyaltyAccounts a ON g.Id = a.GuestId 
                    WHERE g.Email = @Email";

                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Email", email);
                using var reader = await cmd.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    return Results.Ok(new
                    {
                        Cliente = reader.GetString(0),
                        Email = reader.GetString(1),
                        Puntos = reader.GetInt32(2),
                        Nivel = reader.GetInt32(2) > 500 ? "VIP Gold" : "Miembro",
                        Mensaje = "¡Bienvenido!"
                    });
                }
                return Results.NotFound(new { Mensaje = "No encontramos un cliente con ese correo." });
            }
            catch (Exception ex) { return Results.BadRequest($"Error: {ex.Message}"); }
        });

        // GET: Historial
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

        // GET: Premios
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

        // POST: Redimir
        group.MapPost("/redeem", async (RedeemRewardRequest request, IConfiguration config) =>
        {
            try
            {
                var connectionString = string.Format(config.GetConnectionString("TenantTemplate")!, "CONCEPT_HOTEL_DB");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

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

                var updateQuery = "UPDATE LoyaltyAccounts SET PointsBalance = PointsBalance - @Points WHERE Id = @Id";
                using (var updateCmd = new SqlCommand(updateQuery, connection))
                {
                    updateCmd.Parameters.AddWithValue("@Points", request.PointsToRedeem);
                    updateCmd.Parameters.AddWithValue("@Id", accountId);
                    await updateCmd.ExecuteNonQueryAsync();
                }

                var insertTx = "INSERT INTO PointTransactions (Id, LoyaltyAccountId, Points, Type, Description, CreatedAtUtc) VALUES (NEWID(), @AccountId, @Points, 'Redeem', @Desc, GETUTCDATE())";
                using (var txCmd = new SqlCommand(insertTx, connection))
                {
                    txCmd.Parameters.AddWithValue("@AccountId", accountId);
                    txCmd.Parameters.AddWithValue("@Points", -request.PointsToRedeem);
                    txCmd.Parameters.AddWithValue("@Desc", $"Canje: {request.RewardName}");
                    await txCmd.ExecuteNonQueryAsync();
                }

                if (request.IsPhysical && !string.IsNullOrWhiteSpace(request.ShippingAddress))
                {
                    var insertOrder = "INSERT INTO RedemptionOrders (GuestId, RewardId, ShippingAddress) VALUES (@GuestId, @RewardId, @Address)";
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

        // GET: Branding
        group.MapGet("/branding", async (string tenantId, PlatformDbContext platformDb) =>
        {
            if (!Guid.TryParse(tenantId, out var parsedTenantId))
                return Results.BadRequest("TenantId inválido.");

            var tenant = await platformDb.Tenants.FirstOrDefaultAsync(t => t.Id == parsedTenantId);
            
            if (tenant == null) 
                return Results.NotFound("Hotel no encontrado.");

            return Results.Ok(new
            {
                Name = tenant.Name,
                LogoUrl = tenant.LogoUrl ?? "",
                PrimaryColor = tenant.PrimaryColorHex ?? "#4F46E5",
                SecondaryColor = tenant.SecondaryColorHex ?? "#8B5CF6"
            });
        });

        // POST: Solicitar OTP
        group.MapPost("/auth/request-otp", async (OtpRequest request, IConfiguration config, PlatformDbContext platformDb) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email)) return Results.BadRequest("El correo es requerido.");

            try
            {
                var tenant = await platformDb.Tenants.FirstOrDefaultAsync();
                var primaryColor = tenant?.PrimaryColorHex ?? "#4F46E5";
                var logoUrl = tenant?.LogoUrl ?? "";
                var hotelName = tenant?.Name ?? "Loyalty Benefit";

                var connectionString = string.Format(config.GetConnectionString("TenantTemplate")!, "CONCEPT_HOTEL_DB");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var checkQuery = "SELECT Id FROM Guests WHERE Email = @Email AND IsActive = 1";
                using (var checkCmd = new SqlCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@Email", request.Email);
                    var exists = await checkCmd.ExecuteScalarAsync();
                    if (exists == null) return Results.NotFound("El correo no está registrado.");
                }

                var random = new Random();
                var otpCode = random.Next(100000, 999999).ToString();
                var expiresAt = DateTime.UtcNow.AddMinutes(5);

                var updateQuery = "UPDATE Guests SET OtpCode = @Otp, OtpExpiresAt = @Expires WHERE Email = @Email";
                using (var updateCmd = new SqlCommand(updateQuery, connection))
                {
                    updateCmd.Parameters.AddWithValue("@Otp", otpCode);
                    updateCmd.Parameters.AddWithValue("@Expires", expiresAt);
                    updateCmd.Parameters.AddWithValue("@Email", request.Email);
                    await updateCmd.ExecuteNonQueryAsync();
                }

                try
                {
                    var senderEmail = config["EmailSettings:SenderEmail"];
                    var senderName = config["EmailSettings:SenderName"];
                    var smtpServer = config["EmailSettings:SmtpServer"];
                    var smtpPort = int.Parse(config["EmailSettings:SmtpPort"] ?? "2525");
                    var username = config["EmailSettings:Username"];
                    var password = config["EmailSettings:Password"];

                    var fullLogoUrl = string.IsNullOrEmpty(logoUrl) ? "" : $"https://api.loyaltybenefit.com{logoUrl}";

                    using var mail = new System.Net.Mail.MailMessage();
                    mail.From = new System.Net.Mail.MailAddress(senderEmail!, senderName);
                    mail.To.Add(request.Email);
                    mail.Subject = $"Tu código de acceso a {hotelName}";
                    mail.Body = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto; background-color: #0F172A; color: #FFFFFF; border-radius: 16px; overflow: hidden;'>
                            <div style='background-color: {primaryColor}; padding: 30px; text-align: center;'>
                                {(string.IsNullOrEmpty(fullLogoUrl) 
                                    ? $"<h1 style='color: white; margin: 0; font-size: 28px;'>{hotelName}</h1>" 
                                    : $"<img src='{fullLogoUrl}' alt='{hotelName}' style='max-height: 60px; margin-bottom: 10px;' />")}
                            </div>
                            <div style='padding: 40px 30px; text-align: center;'>
                                <h2 style='color: #FFFFFF; margin-top: 0;'>¡Hola de nuevo!</h2>
                                <p style='color: #94A3B8; font-size: 16px; line-height: 1.5;'>Bienvenido al programa de lealtad de <strong>{hotelName}</strong>.</p>
                                <p style='color: #94A3B8; font-size: 14px;'>Para acceder a tu cuenta y consultar tus puntos, usa el siguiente código:</p>
                                <div style='background-color: #1E293B; padding: 20px; border-radius: 12px; margin: 30px 0; border: 1px solid #334155;'>
                                    <h1 style='color: {primaryColor}; letter-spacing: 12px; font-size: 40px; margin: 0;'>{otpCode}</h1>
                                </div>
                                <p style='color: #94A3B8; font-size: 14px;'>Este código expirará en 5 minutos.</p>
                            </div>
                            <div style='background-color: #0F172A; padding: 20px; text-align: center; border-top: 1px solid #1E293B;'>
                                <p style='color: #64748B; font-size: 12px; margin: 0;'>Powered by <strong style='color: #6366F1;'>Loyalty Benefit</strong></p>
                            </div>
                        </div>";
                    mail.IsBodyHtml = true;

                    using var smtpClient = new System.Net.Mail.SmtpClient(smtpServer, smtpPort);
                    smtpClient.Credentials = new System.Net.NetworkCredential(username, password);
                    smtpClient.EnableSsl = true;
                    
                    await smtpClient.SendMailAsync(mail);
                }
                catch (Exception mailEx)
                {
                    Console.WriteLine($"❌ Error SMTP2GO: {mailEx.Message}");
                }

                return Results.Ok(new { mensaje = "Código enviado", debug_otp = otpCode });
            }
            catch (Exception ex) { return Results.BadRequest($"Error: {ex.Message}"); }
        });

        // POST: Verificar OTP
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
                      AND OtpCode = @Code";

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

public record OtpRequest(string Email);
public record OtpVerifyRequest(string Email, string Code);
public record RedeemRewardRequest(
    string TenantId, string GuestEmail, string RewardName, int PointsToRedeem,
    bool IsPhysical = false, string? ShippingAddress = null, Guid? RewardId = null
);
