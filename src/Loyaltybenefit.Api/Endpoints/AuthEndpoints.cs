using Loyaltybenefit.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Loyaltybenefit.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Autenticación");

        group.MapPost("/login", async (LoginRequest request, IConfiguration config) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return Results.BadRequest("Correo y contraseña son requeridos.");

            try
            {
                var connectionString = config.GetConnectionString("Platform")!;
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = "SELECT Id, Email, PasswordHash, Role, PropertyId FROM Users WHERE Email = @Email AND IsActive = 1";
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Email", request.Email);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var userId = reader.GetGuid(0);
                    var email = reader.GetString(1);
                    var hash = reader.GetString(2);
                    var role = reader.GetString(3);
                    var propertyId = reader.IsDBNull(4) ? (Guid?)null : reader.GetGuid(4);

                    // Validar contraseña con BCrypt
                    if (!BCrypt.Net.BCrypt.Verify(request.Password, hash))
                        return Results.Unauthorized();

                    // Generar Token JWT
                    var jwtSecret = config["Jwt:SecretKey"]!;
                    var key = Encoding.ASCII.GetBytes(jwtSecret);
                    
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                            new Claim(ClaimTypes.Email, email),
                            new Claim(ClaimTypes.Role, role),
                            // Si tiene propiedad, la agregamos al token (opcional, pero útil)
                            new Claim("PropertyId", propertyId?.ToString() ?? "") 
                        }),
                        Expires = DateTime.UtcNow.AddMinutes(double.Parse(config["Jwt:ExpirationMinutes"] ?? "1440")),
                        Issuer = config["Jwt:Issuer"],
                        Audience = config["Jwt:Audience"],
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };

                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    var jwtToken = tokenHandler.WriteToken(token);

                    // Devolvemos el token y los datos del usuario
                    return Results.Ok(new
                    {
                        token = jwtToken,
                        role = role,
                        propertyId = propertyId, // El frontend usará esto
                        email = email
                    });
                }

                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                return Results.BadRequest($"Error: {ex.Message}");
            }
        });
    }
}

public record LoginRequest(string Email, string Password);
