using Loyaltybenefit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Loyaltybenefit.Api.Endpoints;

public static class RuleEndpoints
{
    public static void MapRuleEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/rules")
            .WithTags("Reglas de Lealtad")
            .RequireAuthorization();

        // POST: Guardar o actualizar la regla
        group.MapPost("/", async (
            CreateRuleRequest request, 
            PlatformDbContext platformDb,
            IConfiguration config) =>
        {
            try
            {
                var tenant = await platformDb.Tenants.FindAsync(request.TenantId);
                if (tenant == null) return Results.BadRequest("Hotel no encontrado.");

                var tenantDbInfo = await platformDb.TenantDatabases
                    .FirstOrDefaultAsync(td => td.TenantId == request.TenantId && td.IsActive);
                
                if (tenantDbInfo == null) return Results.BadRequest("El hotel no tiene BD asignada.");

                var connectionTemplate = config.GetConnectionString("TenantTemplate");
                var connectionString = string.Format(connectionTemplate!, tenantDbInfo.PhysicalDatabaseName);
                
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var checkRule = "SELECT COUNT(1) FROM LoyaltyRules WHERE IsActive = 1";
                    using (var cmd = new SqlCommand(checkRule, connection))
                    {
                        var exists = (int)(await cmd.ExecuteScalarAsync() ?? 0);

                        if (exists > 0)
                        {
                            var updateRule = "UPDATE LoyaltyRules SET AmountThreshold = @Threshold, Points = @Points WHERE IsActive = 1";
                            using (var updateCmd = new SqlCommand(updateRule, connection))
                            {
                                updateCmd.Parameters.AddWithValue("@Threshold", request.AmountThreshold);
                                updateCmd.Parameters.AddWithValue("@Points", request.Points);
                                await updateCmd.ExecuteNonQueryAsync();
                            }
                        }
                        else
                        {
                            var insertRule = "INSERT INTO LoyaltyRules (Id, Name, Code, AmountThreshold, Points, RedemptionAmount, IsActive, CreatedAtUtc) VALUES (@Id, @Name, @Code, @Threshold, @Points, @RedemptionAmount, 1, GETUTCDATE())";
                            using (var insertCmd = new SqlCommand(insertRule, connection))
                            {
                                insertCmd.Parameters.AddWithValue("@Id", Guid.NewGuid());
                                insertCmd.Parameters.AddWithValue("@Name", "Regla Base");
                                insertCmd.Parameters.AddWithValue("@Code", "REGLA_BASE");
                                insertCmd.Parameters.AddWithValue("@Threshold", request.AmountThreshold);
                                insertCmd.Parameters.AddWithValue("@Points", request.Points);
                                insertCmd.Parameters.AddWithValue("@RedemptionAmount", 0);
                                await insertCmd.ExecuteNonQueryAsync();
                            }
                        }
                    }

                    return Results.Ok(new { 
                        Mensaje = $"Regla configurada: ${request.AmountThreshold} = {request.Points} puntos",
                        Tenant = tenant.Name
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR EN REGLAS: {ex.Message}");
                return Results.BadRequest($"Error: {ex.Message}");
            }
        });

        // GET: Obtener la regla actual
        group.MapGet("/{tenantId:guid}", async (Guid tenantId, PlatformDbContext platformDb, IConfiguration config) =>
        {
            try
            {
                var tenantDbInfo = await platformDb.TenantDatabases
                    .FirstOrDefaultAsync(td => td.TenantId == tenantId && td.IsActive);
                
                if (tenantDbInfo == null) return Results.NotFound("El hotel no tiene BD asignada.");

                var connectionTemplate = config.GetConnectionString("TenantTemplate");
                var connectionString = string.Format(connectionTemplate!, tenantDbInfo.PhysicalDatabaseName);
                
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = "SELECT TOP 1 AmountThreshold, Points FROM LoyaltyRules WHERE IsActive = 1";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return Results.Ok(new { 
                                    amountThreshold = reader.GetDecimal(0),
                                    points = reader.GetInt32(1)
                                });
                            }
                        }
                    }
                    return Results.NotFound("No hay regla configurada.");
                }
            }
            catch (Exception ex)
            {
                return Results.BadRequest($"Error: {ex.Message}");
            }
        });
    }
}

public record CreateRuleRequest(Guid TenantId, decimal AmountThreshold, int Points);
