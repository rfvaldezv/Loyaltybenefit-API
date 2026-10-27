using Loyaltybenefit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Loyaltybenefit.Api.Endpoints;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tenants")
            .WithTags("Tenants (Hoteles)");

        // GET: Listar todos los tenants
        group.MapGet("/", async (PlatformDbContext db) =>
        {
            var tenants = await db.Tenants
                .Select(t => new
                {
                    t.Id,
                    t.Code,
                    t.Name,
                    t.IsActive,
                    t.LogoUrl,
                    t.PrimaryColorHex,
                    t.SecondaryColorHex
                })
                .ToListAsync();

            return Results.Ok(tenants);
        }).RequireAuthorization();

        // POST: Subir Logo del Hotel
        group.MapPost("/{tenantId}/upload-logo", async (
            Guid tenantId,
            [FromForm] IFormFile file,
            IWebHostEnvironment env,
            PlatformDbContext platformDb) =>
        {
            if (file == null || file.Length == 0)
                return Results.BadRequest("No se envió ninguna imagen.");

            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".svg" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return Results.BadRequest("Formato no válido. Usa PNG, JPG o SVG.");

            try
            {
                var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
                var uploadsFolder = Path.Combine(webRoot, "logos");
                
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{tenantId}_{Guid.NewGuid().ToString("N").Substring(0, 8)}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var tenant = await platformDb.Tenants.FindAsync(tenantId);
                if (tenant == null) return Results.NotFound("Hotel no encontrado.");

                var logoUrl = $"/logos/{fileName}";
                tenant.UpdateBranding(logoUrl, tenant.PrimaryColorHex, tenant.SecondaryColorHex);
                
                await platformDb.SaveChangesAsync();

                return Results.Ok(new { mensaje = "Logo subido exitosamente", logoUrl = logoUrl });
            }
            catch (Exception ex)
            {
                return Results.BadRequest($"Error al subir imagen: {ex.Message}");
            }
        })
        .DisableAntiforgery()
        .RequireAuthorization();
    }
}
