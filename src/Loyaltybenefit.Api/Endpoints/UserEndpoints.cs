using Loyaltybenefit.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;

namespace Loyaltybenefit.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Usuarios (Administradores)")
            .RequireAuthorization();

        // GET: Listar usuarios
        group.MapGet("/", async (IConfiguration config) =>
        {
            try
            {
                var connectionString = config.GetConnectionString("Platform")!;
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = "SELECT Id, Email, Role, IsActive FROM Users ORDER BY Email";
                using var cmd = new SqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var users = new List<object>();
                while (await reader.ReadAsync())
                {
                    users.Add(new
                    {
                        Id = reader.GetGuid(0),
                        Email = reader.GetString(1),
                        Role = reader.GetString(2),
                        IsActive = reader.GetBoolean(3)
                    });
                }
                return Results.Ok(users);
            }
            catch (Exception ex) { return Results.BadRequest($"Error: {ex.Message}"); }
        });

        // POST: Crear usuario
        group.MapPost("/", async (CreateUserRequest request, IConfiguration config) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return Results.BadRequest("El correo y la contraseña son obligatorios.");

            try
            {
                var connectionString = config.GetConnectionString("Platform")!;
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using (var checkCmd = new SqlCommand("SELECT COUNT(1) FROM Users WHERE Email = @Email", connection))
                {
                    checkCmd.Parameters.AddWithValue("@Email", request.Email);
                    var exists = (int)(await checkCmd.ExecuteScalarAsync() ?? 0);
                    if (exists > 0) return Results.Conflict("El correo ya está registrado.");
                }

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                var id = Guid.NewGuid();
                
                var query = @"INSERT INTO Users (Id, Email, PasswordHash, Role, IsActive, CreatedAtUtc) 
                              VALUES (@Id, @Email, @Hash, @Role, 1, GETUTCDATE())";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Email", request.Email);
                    cmd.Parameters.AddWithValue("@Hash", passwordHash);
                    cmd.Parameters.AddWithValue("@Role", request.Role);
                    await cmd.ExecuteNonQueryAsync();
                }

                return Results.Ok(new { mensaje = "Usuario creado", id = id });
            }
            catch (Exception ex) { return Results.BadRequest($"Error: {ex.Message}"); }
        });

        // PUT: Editar usuario
        group.MapPut("/{id}", async (Guid id, UpdateUserRequest request, IConfiguration config) =>
        {
            try
            {
                var connectionString = config.GetConnectionString("Platform")!;
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = "UPDATE Users SET Role = @Role, IsActive = @IsActive WHERE Id = @Id";
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Role", request.Role);
                cmd.Parameters.AddWithValue("@IsActive", request.IsActive);
                
                if (!string.IsNullOrWhiteSpace(request.Password))
                {
                    var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                    query = "UPDATE Users SET Role = @Role, IsActive = @IsActive, PasswordHash = @Hash WHERE Id = @Id";
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@Hash", hash);
                }

                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0) return Results.NotFound("Usuario no encontrado.");

                return Results.Ok(new { mensaje = "Usuario actualizado" });
            }
            catch (Exception ex) { return Results.BadRequest($"Error: {ex.Message}"); }
        });

        // DELETE: Eliminar (desactivar) usuario
        group.MapDelete("/{id}", async (Guid id, IConfiguration config) =>
        {
            try
            {
                var connectionString = config.GetConnectionString("Platform")!;
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = "UPDATE Users SET IsActive = 0 WHERE Id = @Id";
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Id", id);
                
                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0) return Results.NotFound("Usuario no encontrado.");

                return Results.Ok(new { mensaje = "Usuario desactivado" });
            }
            catch (Exception ex) { return Results.BadRequest($"Error: {ex.Message}"); }
        });
    }
}

// DTOs (Fuera de la clase, al final del archivo)
public record CreateUserRequest(string Email, string Password, string Role);
public record UpdateUserRequest(string Role, bool IsActive, string? Password = null);
