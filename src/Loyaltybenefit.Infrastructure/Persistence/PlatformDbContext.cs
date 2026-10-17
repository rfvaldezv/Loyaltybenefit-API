using Loyaltybenefit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Loyaltybenefit.Infrastructure.Persistence;

public sealed class PlatformDbContext : DbContext
{
    public PlatformDbContext(
        DbContextOptions<PlatformDbContext> options)
        : base(options)
    {
    }

    public DbSet<Industry> Industries =>
        Set<Industry>();

    public DbSet<Tenant> Tenants =>
        Set<Tenant>();

    public DbSet<TenantDatabase> TenantDatabases =>
        Set<TenantDatabase>();

    public DbSet<DatabaseServer> DatabaseServers =>
        Set<DatabaseServer>();

    public DbSet<DatabaseCredential> DatabaseCredentials =>
        Set<DatabaseCredential>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(
            new Configurations.IndustryConfiguration());

        modelBuilder.ApplyConfiguration(
            new Configurations.TenantConfiguration());

        modelBuilder.ApplyConfiguration(
            new Configurations.TenantDatabaseConfiguration());

        modelBuilder.ApplyConfiguration(
            new Configurations.DatabaseServerConfiguration());

        modelBuilder.ApplyConfiguration(
            new Configurations.DatabaseCredentialConfiguration());
    }
}
