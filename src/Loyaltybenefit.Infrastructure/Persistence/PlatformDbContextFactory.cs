using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Loyaltybenefit.Infrastructure.Persistence;

public class PlatformDbContextFactory
    : IDesignTimeDbContextFactory<PlatformDbContext>
{
    public PlatformDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                "Loyaltybenefit_PLATFORM_CONNECTION");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Loyaltybenefit_PLATFORM_CONNECTION is not configured.");
        }

        var optionsBuilder =
            new DbContextOptionsBuilder<PlatformDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new PlatformDbContext(
            optionsBuilder.Options);
    }
}
