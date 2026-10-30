using Loyaltybenefit.Domain.Entities;
using Loyaltybenefit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace Loyaltybenefit.Api.Endpoints;

public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/customers")
            .WithTags("Clientes (Huéspedes)");
            // .RequireAuthorization(); // Comentado para permitir el registro desde el Kiosko

        // GET: Listar clientes desde SQL Server
        group.MapGet("/", async (
            HttpContext httpContext,
            PlatformDbContext platformDb,
            IConfiguration config,
            string? tenantId = null) =>
        {
            try
            {
                var connectionTemplate = config.GetConnectionString("TenantTemplate");
                var connectionString = string.Format(connectionTemplate!, "CONCEPT_HOTEL_DB");

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT Id, FirstName, LastName, Email, Phone, CreatedAtUtc FROM Guests WHERE IsActive = 1";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            var guests = new List<object>();
                            while (await reader.ReadAsync())
                            {
                                guests.Add(new
                                {
                                    Id = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0),
                                    FirstName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                    LastName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                    Email = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                    Phone = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                    CreatedAtUtc = reader.IsDBNull(5) ? DateTime.UtcNow : reader.GetDateTime(5)
                                });
                            }
                            return Results.Ok(guests);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // ESTO NOS DIRÁ EL ERROR REAL EN LA CONSOLA Y EN FLUTTER
                Console.WriteLine($"❌ ERROR AL CREAR CLIENTE: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"❌ INNER: {ex.InnerException.Message}");
                    
                return Results.BadRequest($"Error exacto: {ex.Message} - {ex.InnerException?.Message}");
            }
        });

        // POST: Registrar un cliente (SIN validación de Header para permitir el Kiosko)
        group.MapPost("/", async (
            CreateCustomerRequest request,
            PlatformDbContext platformDb,
            IConfiguration config) =>
        {
            var firstName = request.GetFirstName();
            var lastName = request.GetLastName();
            var email = request.Email;

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(email))
                return Results.BadRequest("El nombre y el correo son obligatorios.");

            try
            {
                var connectionTemplate = config.GetConnectionString("TenantTemplate");
                var connectionString = string.Format(connectionTemplate!, "CONCEPT_HOTEL_DB");

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Verificar si ya existe
                    using (var checkCmd = new SqlCommand("SELECT COUNT(1) FROM Guests WHERE Email = @Email", connection))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", email);
                        var exists = (int)(await checkCmd.ExecuteScalarAsync() ?? 0);
                        if (exists > 0) 
                            return Results.Conflict("El correo ya está registrado."); // <-- Cambiamos BadRequest por Conflict (409)
                    }

                    var guestId = Guid.NewGuid();
                    var query = "INSERT INTO Guests (Id, FirstName, LastName, Email, Phone, CreatedAtUtc, IsActive) VALUES (@Id, @FirstName, @LastName, @Email, @Phone, GETUTCDATE(), 1)";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", guestId);
                        cmd.Parameters.AddWithValue("@FirstName", firstName);
                        cmd.Parameters.AddWithValue("@LastName", lastName);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Phone", (object?)request.Phone ?? DBNull.Value);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Crear la cuenta de lealtad para el cliente
                    var accountId = Guid.NewGuid();
                    var insertAccount = "INSERT INTO LoyaltyAccounts (Id, GuestId, PointsBalance) VALUES (@Id, @GuestId, 0)";
                    using (var accCmd = new SqlCommand(insertAccount, connection))
                    {
                        accCmd.Parameters.AddWithValue("@Id", accountId);
                        accCmd.Parameters.AddWithValue("@GuestId", guestId);
                        await accCmd.ExecuteNonQueryAsync();
                    }

                    return Results.Ok(new
                    {
                        Id = guestId,
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email
                    });
                }
            }
            catch (Exception ex)
            {
                return Results.BadRequest($"Error al guardar en SQL: {ex.Message}");
            }
        });

        // POST: Ajuste manual de puntos
        group.MapPost("/{guestId}/adjust-points", async (
            Guid guestId,
            AdjustPointsRequest request,
            IConfiguration config) =>
        {
            if (request.Points == 0)
                return Results.BadRequest("Los puntos no pueden ser cero.");
            
            if (string.IsNullOrWhiteSpace(request.Reason))
                return Results.BadRequest("La razón del ajuste es obligatoria.");

            try
            {
                var connectionTemplate = config.GetConnectionString("TenantTemplate");
                var connectionString = string.Format(connectionTemplate!, "CONCEPT_HOTEL_DB");

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var queryAccount = "SELECT Id, PointsBalance FROM LoyaltyAccounts WHERE GuestId = @GuestId";
                    Guid? accountId = null;
                    int currentPoints = 0;

                    using (var cmd = new SqlCommand(queryAccount, connection))
                    {
                        cmd.Parameters.AddWithValue("@GuestId", guestId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                accountId = reader.GetGuid(0);
                                currentPoints = reader.GetInt32(1);
                            }
                        }
                    }

                    if (accountId == null)
                        return Results.NotFound("No se encontró una cuenta de lealtad para este cliente.");

                    var updateQuery = "UPDATE LoyaltyAccounts SET PointsBalance = PointsBalance + @Points WHERE Id = @Id";
                    using (var updateCmd = new SqlCommand(updateQuery, connection))
                    {
                        updateCmd.Parameters.AddWithValue("@Points", request.Points);
                        updateCmd.Parameters.AddWithValue("@Id", accountId.Value);
                        await updateCmd.ExecuteNonQueryAsync();
                    }

                    var insertTxQuery = @"
                        INSERT INTO PointTransactions (Id, LoyaltyAccountId, Points, Type, Description, CreatedAtUtc) 
                        VALUES (NEWID(), @AccountId, @Points, @Type, @Description, GETUTCDATE())";
                    
                    using (var txCmd = new SqlCommand(insertTxQuery, connection))
                    {
                        txCmd.Parameters.AddWithValue("@AccountId", accountId.Value);
                        txCmd.Parameters.AddWithValue("@Points", request.Points);
                        txCmd.Parameters.AddWithValue("@Type", request.Points > 0 ? "ManualEarn" : "ManualRedeem");
                        txCmd.Parameters.AddWithValue("@Description", $"Ajuste manual: {request.Reason}");
                        await txCmd.ExecuteNonQueryAsync();
                    }

                    return Results.Ok(new
                    {
                        mensaje = "Ajuste realizado con éxito",
                        nuevoSaldo = currentPoints + request.Points,
                        puntosAjustados = request.Points
                    });
                }
            }
            catch (Exception ex)
            {
                return Results.BadRequest($"Error al ajustar puntos: {ex.Message}");
            }
        });
    }
}

public record CreateCustomerRequest(
    string? FirstName = null,
    string? LastName = null,
    string? Email = null,
    string? Phone = null,
    string? Name = null
)
{
    public string GetFirstName() =>
        !string.IsNullOrWhiteSpace(FirstName)
            ? FirstName
            : (Name?.Split(' ').FirstOrDefault() ?? "Huésped");

    public string GetLastName() =>
        !string.IsNullOrWhiteSpace(LastName)
            ? LastName
            : (Name != null && Name.Contains(' ')
                ? Name.Substring(Name.IndexOf(' ') + 1)
                : "");
}

public record AdjustPointsRequest(
    int Points,
    string Reason
);
