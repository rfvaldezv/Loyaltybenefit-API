using Loyaltybenefit.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;

namespace Loyaltybenefit.Api.Endpoints;

public static class PropertyEndpoints
{
    public static void MapPropertyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/properties")
            .WithTags("Propiedades (Sucursales)")
            .RequireAuthorization();

        // GET: Listar todas las propiedades (filtrado por rol)
        group.MapGet("/", async (HttpContext httpContext, IConfiguration config) =>
        {
            try
            {
                // 1. Obtenemos el rol y el PropertyId del Token JWT (que ya configuramos para que los devuelva)
                var userRole = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                var userPropertyId = httpContext.User.FindFirst("PropertyId")?.Value;

                var connectionString = string.Format(config.GetConnectionString("TenantTemplate")!, "CONCEPT_HOTEL_DB");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                string query;
                SqlCommand cmd;

                // 2. Si es Admin de Hotel, solo le mostramos su hotel.
                if (userRole == "TenantAdmin" && !string.IsNullOrEmpty(userPropertyId))
                {
                    query = "SELECT Id, Name, Code, PrimaryColorHex, SecondaryColorHex, IsActive FROM Properties WHERE Id = @PropertyId";
                    cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@PropertyId", Guid.Parse(userPropertyId));
                }
                else
                {
                    // Si es SuperAdmin, ve todo.
                    query = "SELECT Id, Name, Code, PrimaryColorHex, SecondaryColorHex, IsActive FROM Properties ORDER BY Name";
                    cmd = new SqlCommand(query, connection);
                }

                using (cmd)
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    var properties = new List<object>();
                    while (await reader.ReadAsync())
                    {
                        properties.Add(new
                        {
                            Id = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            Code = reader.GetString(2),
                            PrimaryColorHex = reader.IsDBNull(3) ? "#4F46E5" : reader.GetString(3),
                            SecondaryColorHex = reader.IsDBNull(4) ? "#8B5CF6" : reader.GetString(4),
                            IsActive = reader.GetBoolean(5)
                        });
                    }
                    return Results.Ok(properties);
                }
            }
            catch (Exception ex) { return Results.BadRequest($"Error: {ex.Message}"); }
        });

        // POST: Crear
        group.MapPost("/", async (CreatePropertyRequest request, IConfiguration config) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Code))
                return Results.BadRequest("El nombre y el código son obligatorios.");

            try
            {
                var connectionString = string.Format(config.GetConnectionString("TenantTemplate")!, "CONCEPT_HOTEL_DB");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using (var checkCmd = new SqlCommand("SELECT COUNT(1) FROM Properties WHERE Code = @Code", connection))
                {
                    checkCmd.Parameters.AddWithValue("@Code", request.Code);
                    var exists = (int)(await checkCmd.ExecuteScalarAsync() ?? 0);
                    if (exists > 0) return Results.Conflict("El código de propiedad ya existe.");
                }

                var id = Guid.NewGuid();
                var query = @"INSERT INTO Properties (Id, Name, Code, PrimaryColorHex, SecondaryColorHex, IsActive, CreatedAtUtc) 
                              VALUES (@Id, @Name, @Code, @Primary, @Secondary, 1, GETUTCDATE())";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Name", request.Name);
                    cmd.Parameters.AddWithValue("@Code", request.Code);
                    cmd.Parameters.AddWithValue("@Primary", (object?)request.PrimaryColorHex ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Secondary", (object?)request.SecondaryColorHex ?? DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }

                return Results.Ok(new { mensaje = "Propiedad creada exitosamente", id = id });
            }
            catch (Exception ex) { return Results.BadRequest($"Error: {ex.Message}"); }
        });

        // PUT: Editar
        group.MapPut("/{id}", async (Guid id, UpdatePropertyRequest request, IConfiguration config) =>
        {
            try
            {
                var connectionString = string.Format(config.GetConnectionString("TenantTemplate")!, "CONCEPT_HOTEL_DB");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = "UPDATE Properties SET Name = @Name, Code = @Code WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Name", request.Name);
                    cmd.Parameters.AddWithValue("@Code", request.Code);
                    
                    int rows = await cmd.ExecuteNonQueryAsync();
                    if (rows == 0) return Results.NotFound("Propiedad no encontrada.");
                }

                return Results.Ok(new { mensaje = "Propiedad actualizada exitosamente" });
            }
            catch (Exception ex) { return Results.BadRequest($"Error: {ex.Message}"); }
        });

        // DELETE: Eliminar
        group.MapDelete("/{id}", async (Guid id, IConfiguration config) =>
        {
            try
            {
                var connectionString = string.Format(config.GetConnectionString("TenantTemplate")!, "CONCEPT_HOTEL_DB");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = "UPDATE Properties SET IsActive = 0 WHERE Id = @Id";
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Id", id);
                
                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0) return Results.NotFound("Propiedad no encontrada.");

                return Results.Ok(new { mensaje = "Propiedad desactivada correctamente" });
            }
            catch (Exception ex) { return Results.BadRequest($"Error: {ex.Message}"); }
        });
    }
}

// DTOs
public record CreatePropertyRequest(string Name, string Code, string? PrimaryColorHex = null, string? SecondaryColorHex = null);
public record UpdatePropertyRequest(string Name, string Code);
