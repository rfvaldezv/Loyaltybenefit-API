using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Loyaltybenefit.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/login", (LoginRequest request, IConfiguration config) =>
        {
            // 1. Validación básica (Hardcodeada para pruebas iniciales)
            // En el futuro, esto consultará a la base de datos (Platform DB)
            if (request.Email != "admin@loyaltybenefit.com" || request.Password != "Admin123!")
            {
                return Results.Unauthorized();
            }

            // 2. Crear los Claims (la información del usuario dentro del token)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, request.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, "Admin de Prueba"),
                new Claim(ClaimTypes.Role, "SuperAdmin")
            };

            // 3. Leer configuración
            var secretKey = config["Jwt:SecretKey"] ?? throw new InvalidOperationException("Falta Jwt:SecretKey");
            var issuer = config["Jwt:Issuer"];
            var audience = config["Jwt:Audience"];
            var expirationMinutes = int.Parse(config["Jwt:ExpirationMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 4. Generar el Token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // 5. Devolver respuesta
            return Results.Ok(new
            {
                token = tokenString,
                email = request.Email,
                nombre = "Admin de Prueba",
                expira = token.ValidTo
            });
        });
    }
}

// DTO para recibir los datos
public record LoginRequest(string Email, string Password);
