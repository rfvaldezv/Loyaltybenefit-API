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

        // GET: Dashboard Corporativo o por Propiedad
        group.MapGet("/corporate", async (IConfiguration config, Guid? propertyId) =>
        {
            var connectionString = string.Format(config.GetConnectionString("TenantTemplate")!, "CONCEPT_HOTEL_DB");

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                string filter = propertyId.HasValue ? "WHERE pt.PropertyId = @PropertyId" : "";

                // 1. KPIs Globales
                var kpiQuery = $@"
                    SELECT 
                        COUNT(DISTINCT g.Id) as TotalClientes,
                        SUM(CASE WHEN pt.Points > 0 THEN pt.Points ELSE 0 END) as TotalPuntosOtorgados,
                        SUM(CASE WHEN pt.Points < 0 THEN pt.Points * -1 ELSE 0 END) as TotalPuntosCanjeados
                    FROM Guests g
                    LEFT JOIN PointTransactions pt ON g.Id = (SELECT GuestId FROM LoyaltyAccounts WHERE Id = pt.LoyaltyAccountId)
                    {filter}";

                int totalClientes = 0;
                int puntosOtorgados = 0;
                int puntosCanjeados = 0;

                using (var cmd = new SqlCommand(kpiQuery, connection))
                {
                    if (propertyId.HasValue) cmd.Parameters.AddWithValue("@PropertyId", propertyId.Value);
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            totalClientes = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                            puntosOtorgados = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                            puntosCanjeados = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                        }
                    }
                }

                // 2. Puntos por Propiedad (Para el gráfico)
                var chartQuery = @"
                    SELECT p.Name, SUM(CASE WHEN pt.Points > 0 THEN pt.Points ELSE 0 END)
                    FROM PointTransactions pt
                    INNER JOIN Properties p ON pt.PropertyId = p.Id
                    GROUP BY p.Name";

                var chartData = new List<object>();
                using (var cmd = new SqlCommand(chartQuery, connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            chartData.Add(new { PropertyName = reader.GetString(0), PuntosGanados = reader.IsDBNull(1) ? 0 : reader.GetInt32(1) });
                        }
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
            catch (Exception ex) { return Results.BadRequest($"Error en Analytics: {ex.Message}"); }
        });
    }
}
