using Loyaltybenefit.Application;
using Loyaltybenefit.Infrastructure;
using Loyaltybenefit.Infrastructure.Persistence;
using Loyaltybenefit.Api.Endpoints;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// Servicios
// ---------------------------------------------------------

builder.Services.AddOpenApi();

// Swagger UI simplificado para .NET 10
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar Autenticación JWT
var jwtSecret = builder.Configuration["Jwt:SecretKey"] ?? throw new Exception("Falta Jwt:SecretKey en appsettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddScoped<Loyaltybenefit.Application.Loyalty.ITenantContext, Loyaltybenefit.Infrastructure.Persistence.TenantContext>();
builder.Services.AddScoped<Loyaltybenefit.Infrastructure.Persistence.TenantDbContextFactory>();

// ---------------------------------------------------------
// Pipeline
// ---------------------------------------------------------

var app = builder.Build();

// Seeder
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    await PlatformSeeder.SeedAsync(context, builder.Configuration);
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Loyalty Benefit API v1");
    options.RoutePrefix = "swagger";
});

// OpenAPI nativo
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// HTTPS solo en producción
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 🔒 MIDDLEWARES DE SEGURIDAD
app.UseAuthentication();
app.UseAuthorization();

// ---------------------------------------------------------
// Endpoints
// ---------------------------------------------------------

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Loyalty Benefit API" }));
app.MapGet("/", () => Results.Ok(new { message = "Loyalty Benefit API", version = "v1", docs = "/swagger" }));

app.MapTenantEndpoints();
app.MapTenantDatabaseEndpoints();
app.MapLoyaltyEndpoints();
app.MapAuthEndpoints();
//app.MapTenantManagementEndpoints();
app.MapCustomerEndpoints();
app.MapStayEndpoints(); // 👈 NUEVO
app.MapGuestPortalEndpoints();  // 👈 NUEVO

app.Run();
