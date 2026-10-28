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

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // ... (tu configuración existente de Tenant, etc.)
    
    // NUEVO: Configuración de Properties
    modelBuilder.Entity<Property>(entity =>
    {
        entity.ToTable("Properties");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
        entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
    });
    
    // NUEVO: Mapear los campos de Branding de Tenant
    modelBuilder.Entity<Tenant>(entity =>
    {
        entity.Property(e => e.AccountType).HasMaxLength(20).HasDefaultValue("Single");
    });
}

    public DbSet<Property> Properties { get; set; }
}
