using Loyaltybenefit.Application;
using Loyaltybenefit.Infrastructure;
using Loyaltybenefit.Infrastructure.Persistence;
using Loyaltybenefit.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(
    builder.Configuration);

builder.Services.AddApplication();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider
            .GetRequiredService<PlatformDbContext>();

    await PlatformSeeder.SeedAsync(
        context,
        builder.Configuration);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapTenantEndpoints();

app.MapTenantDatabaseEndpoints();

app.MapLoyaltyEndpoints();

app.Run();
