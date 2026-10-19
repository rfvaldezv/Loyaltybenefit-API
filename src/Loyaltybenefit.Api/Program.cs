using Loyaltybenefit.Application;
using Loyaltybenefit.Infrastructure;
using Loyaltybenefit.Infrastructure.Persistence;
using Loyaltybenefit.Api.Endpoints;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// Servicios
// ---------------------------------------------------------

builder.Services.AddOpenApi();

// Swagger UI (interfaz visual para probar la API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Loyalty Benefit API",
        Version = "v1",
        Description = "API del SaaS de lealtad Loyalty Benefit"
    });
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// ---------------------------------------------------------
// Pipeline
// ---------------------------------------------------------

var app = builder.Build();

// Seeder (corre al arrancar)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    await PlatformSeeder.SeedAsync(context, builder.Configuration);
}

// Swagger (siempre habilitado para desarrollo y demo)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Loyalty Benefit API v1");
    options.RoutePrefix = "swagger"; // http://localhost:5265/swagger
});

// OpenAPI nativo (JSON en /openapi/v1.json)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// HTTPS solo en producción (en Development no, para evitar conflictos)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// ---------------------------------------------------------
// Endpoints
// ---------------------------------------------------------

// Health check
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "Loyalty Benefit API",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}));

// Raíz (para saber que la API está viva)
app.MapGet("/", () => Results.Ok(new
{
    message = "Loyalty Benefit API",
    version = "v1",
    docs = "/swagger"
}));

// Endpoints de negocio
app.MapTenantEndpoints();
app.MapTenantDatabaseEndpoints();
app.MapLoyaltyEndpoints();

app.Run();
