using Loyaltybenefit.Application;
using Loyaltybenefit.Infrastructure;
using Loyaltybenefit.Infrastructure.Persistence;
using Loyaltybenefit.Api.Endpoints;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuracion de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontends", policy =>
    {
        policy.WithOrigins(
            "https://admin.loyaltybenefit.com",
            "https://app.loyaltybenefit.com",
            "https://kiosko.loyaltybenefit.com", // <-- AGREGADO
            "https://www.loyaltybenefit.com",    // <-- AGREGADO POR SI ACASO
            "http://localhost:5265",
            "http://localhost:3000",
            "http://localhost:8080",
            "http://localhost:8081"              // <-- Por si pruebas en otro puerto local
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

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

var app = builder.Build();

// =====================================================================
// INICIALIZACIÓN DE BASE DE DATOS (Solo Plataforma)
// =====================================================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    await PlatformSeeder.SeedAsync(context, builder.Configuration);
}
// =====================================================================

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Loyalty Benefit API v1");
    options.RoutePrefix = "swagger";
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontends");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Loyalty Benefit API" }));
app.MapGet("/", () => Results.Ok(new { message = "Loyalty Benefit API", version = "v1", docs = "/swagger" }));

app.MapTenantEndpoints();
app.MapTenantDatabaseEndpoints();
app.MapLoyaltyEndpoints();
app.MapAuthEndpoints();
app.MapStayEndpoints();
app.MapGuestPortalEndpoints();
app.MapCustomerEndpoints();
app.MapRuleEndpoints();
app.MapKioskEndpoints();
app.UseStaticFiles();
app.MapAnalyticsEndpoints();

app.Run();
