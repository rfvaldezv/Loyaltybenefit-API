using Loyaltybenefit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Loyaltybenefit.Api.Endpoints;

public static class KioskEndpoints
{
    public static void MapKioskEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/kiosks")
            .WithTags("Kioskos (Tablets)");

        // 1. ENDPOINT ADMIN: Generar Pairing Code
        group.MapPost("/{kioskId}/generate-pairing-code", async (
            Guid kioskId, IConfiguration config) =>
        {
            try
            {
                var connectionString = string.Format(config.GetConnectionString("TenantTemplate")!, "CONCEPT_HOTEL_DB");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var pairingCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                var expiration = DateTime.UtcNow.AddMinutes(15);

                var query = @"
                    UPDATE Kiosks 
                    SET PairingCode = @Code, 
                        PairingCodeExpiresAt = @Expiration,
                        IsPaired = 0 
                    WHERE Id = @Id";

                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Code", pairingCode);
                cmd.Parameters.AddWithValue("@Expiration", expiration);
                cmd.Parameters.AddWithValue("@Id", kioskId);

                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0) return Results.NotFound("Kiosko no encontrado.");

                return Results.Ok(new { 
                    mensaje = "Código generado", 
                    pairingCode = pairingCode, 
                    expiresInMinutes = 15 
                });
            }
            catch (Exception ex) { return Results.BadRequest($"Error: {ex.Message}"); }
        }).RequireAuthorization(); // Requiere JWT de Admin

        // 2. ENDPOINT KIOSKO: Vincularse con el código
        group.MapPost("/pair", async (PairRequest request, IConfiguration config) =>
        {
            try
            {
                var connectionString = string.Format(config.GetConnectionString("TenantTemplate")!, "CONCEPT_HOTEL_DB");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

		var query = @"
		    SELECT Id, KioskToken 
		    FROM Kiosks 
		    WHERE PairingCode = @Code";

                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Code", request.PairingCode.ToUpper());
                
                Guid kioskId;
                string kioskToken;

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        kioskId = reader.GetGuid(0);
                        kioskToken = reader.GetString(1);
                    }
                    else return Results.BadRequest("Código inválido o expirado.");
                }

                var updateQuery = "UPDATE Kiosks SET IsPaired = 1, PairingCode = NULL WHERE Id = @Id";
                using var updateCmd = new SqlCommand(updateQuery, connection);
                updateCmd.Parameters.AddWithValue("@Id", kioskId);
                await updateCmd.ExecuteNonQueryAsync();

                return Results.Ok(new { 
                    mensaje = "Kiosko vinculado exitosamente", 
                    kioskId = kioskId,
                    kioskToken = kioskToken 
                });
            }
            catch (Exception ex) { return Results.BadRequest($"Error: {ex.Message}"); }
        });

        // 3. ENDPOINT KIOSKO: Validar Token (saber si ya está vinculado)
        group.MapGet("/me", async (string kioskToken, IConfiguration config) =>
        {
            try
            {
                var connectionString = string.Format(config.GetConnectionString("TenantTemplate")!, "CONCEPT_HOTEL_DB");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = "SELECT Id, Name FROM Kiosks WHERE KioskToken = @Token AND IsPaired = 1 AND IsActive = 1";
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Token", kioskToken);
                
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return Results.Ok(new { 
                        KioskId = reader.GetGuid(0), 
                        Name = reader.GetString(1), 
                        Status = "Vinculado" 
                    });
                }
                return Results.NotFound("Kiosko no vinculado o token inválido.");
            }
            catch (Exception ex) { return Results.BadRequest($"Error: {ex.Message}"); }
        });
    }
}

public record PairRequest(string PairingCode);
