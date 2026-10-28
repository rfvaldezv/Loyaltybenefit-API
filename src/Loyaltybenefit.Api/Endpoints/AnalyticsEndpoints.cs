using Loyaltybenefit.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;

namespace Loyaltybenefit.Api.Endpoints;

public static class AnalyticsEndpoints
{
    public static void MapAnalyticsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/analytics")
            .WithTags("Analytics (Dashboard)")
            .RequireAuthorization();

        // GET: Dashboard Corporativo (Todos los hoteles aglutinados)
        group.MapGet("/corporate", async (IConfiguration config) =>
        {
            var connectionString = string.Format(config.GetConnectionString("TenantTemplate")!, "CONCEPT_HOTEL_DB");

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // 1. KPIs Globales
                var kpiQuery = @"
                    SELECT 
                        COUNT(DISTINCT g.Id) as TotalClientes,
                        SUM(CASE WHEN pt.Points > 0 THEN pt.Points ELSE 0 END) as TotalPuntosOtorgados,
                        SUM(CASE WHEN pt.Points < 0 THEN pt.Points * -1 ELSE 0 END) as TotalPuntosCanjeados
                    FROM Guests g
                    LEFT JOIN PointTransactions pt ON g.Id = (SELECT GuestId FROM LoyaltyAccounts WHERE Id = pt.LoyaltyAccountId)";

                int totalClientes = 0;
                int puntosOtorgados = 0;
                int puntosCanjeados = 0;

                using (var cmd = new SqlCommand(kpiQuery, connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        totalClientes = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        puntosOtorgados = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        puntosCanjeados = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                    }
                }

                // 2. Puntos por Propiedad (Para el gráfico de barras)
                var chartQuery = @"
                    SELECT 
                        p.Name as PropertyName,
                        SUM(CASE WHEN pt.Points > 0 THEN pt.Points ELSE 0 END) as PuntosGanados
                    FROM PointTransactions pt
                    INNER JOIN Properties p ON pt.PropertyId = p.Id
                    GROUP BY p.Name";

                var chartData = new List<object>();
                using (var cmd = new SqlCommand(chartQuery, connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        chartData.Add(new
                        {
                            PropertyName = reader.GetString(0),
                            PuntosGanados = reader.IsDBNull(1) ? 0 : reader.GetInt32(1)
                        });
                    }
                }

                return Results.Ok(new
                {
                    TotalClientes = totalClientes,
                    PuntosOtorgados = puntosOtorgados,
                    PuntosCanjeados = puntosCanjeados,
                    SaldoActualEnCirculacion = puntosOtorgados - puntosCanjeados,
                    ChartData = chartData
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest($"Error en Analytics: {ex.Message}");
            }
        });

        // GET: Dashboard por Propiedad (Solo un Hotel)
        group.MapGet("/property/{propertyId}", async (Guid propertyId, IConfiguration config) =>
        {
            var connectionString = string.Format(config.GetConnectionString("TenantTemplate")!, "CONCEPT_HOTEL_DB");

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                        SUM(CASE WHEN pt.Points > 0 THEN pt.Points ELSE 0 END) as PuntosGanados,
                        SUM(CASE WHEN pt.Points < 0 THEN pt.Points * -1 ELSE 0 END) as PuntosCanjeados
                    FROM PointTransactions pt
                    WHERE pt.PropertyId = @PropertyId";

                int puntosGanados = 0;
                int puntosCanjeados = 0;

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            puntosGanados = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                            puntosCanjeados = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        }
                    }
                }

                return Results.Ok(new
                {
                    PropertyId = propertyId,
                    PuntosGanados = puntosGanados,
                    PuntosCanjeados = puntosCanjeados,
                    SaldoNeto = puntosGanados - puntosCanjeados
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest($"Error: {ex.Message}");
            }
        });
    }
}
