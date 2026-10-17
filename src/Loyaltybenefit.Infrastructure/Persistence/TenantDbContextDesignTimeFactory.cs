using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Loyaltybenefit.Infrastructure.Persistence;

public sealed class TenantDbContextDesignTimeFactory
    : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<TenantDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\MSSQLLocalDB;Database=Loyaltybenefit_Tenant_DesignTime;Trusted_Connection=True;TrustServerCertificate=True;",
            sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(
                    typeof(TenantDbContext).Assembly.FullName);
            });

        return new TenantDbContext(
            optionsBuilder.Options);
    }
}
