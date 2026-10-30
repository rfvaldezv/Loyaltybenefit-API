using System.Text;
using Loyaltybenefit.Api.Endpoints;
using Loyaltybenefit.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Base de Datos
builder.Services.AddDbContext<PlatformDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Platform")));

// 2. Configuración de JWT (Autenticación)
var jwtSecret = builder.Configuration["Jwt:SecretKey"] ?? "ESTA_ES_UNA_LLAVE_SECRETA_SUPER_LARGA_PARA_PRODUCCION_2026_1234567890";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; 
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "LoyaltyBenefitAPI",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "LoyaltyBenefitApp",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 3. CORS (Permitir TODO en desarrollo)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontends", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. Middlewares (El orden es CRÍTICO)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔥 EL ORDEN CORRECTO: CORS ANTES de StaticFiles
app.UseCors("AllowFrontends");

app.UseStaticFiles(); // Para logos

app.UseAuthentication(); 
app.UseAuthorization();

// 5. Registrar Endpoints
app.MapAuthEndpoints();
app.MapTenantEndpoints();
app.MapCustomerEndpoints();
app.MapGuestPortalEndpoints();
app.MapKioskEndpoints();
app.MapAnalyticsEndpoints();

// Endpoints comentados temporalmente por errores de TenantContext
// app.MapLoyaltyEndpoints(); 
// app.MapRuleEndpoints();    
// app.MapStayEndpoints();    
// app.MapTenantDatabaseEndpoints();
app.MapPropertyEndpoints();
app.MapUserEndpoints();

app.Run();
