using Loyaltybenefit.Domain.Entities;
using Loyaltybenefit.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Loyaltybenefit.Infrastructure.Persistence;

public sealed class TenantDbContext : DbContext
{
    public TenantDbContext(
        DbContextOptions<TenantDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations =>
        Set<Organization>();

    public DbSet<Location> Locations =>
        Set<Location>();

    public DbSet<Guest> Guests =>
        Set<Guest>();

    public DbSet<Stay> Stays =>
        Set<Stay>();

    public DbSet<LoyaltyAccount> LoyaltyAccounts =>
        Set<LoyaltyAccount>();

    public DbSet<LoyaltyRule> LoyaltyRules =>
        Set<LoyaltyRule>();

    public DbSet<PointTransaction> PointTransactions =>
        Set<PointTransaction>();

    public DbSet<Redemption> Redemptions =>
        Set<Redemption>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(
            new GuestConfiguration());

        modelBuilder.ApplyConfiguration(
            new HotelConfiguration());

        modelBuilder.ApplyConfiguration(
            new LoyaltyAccountConfiguration());

        modelBuilder.ApplyConfiguration(
            new LoyaltyRuleConfiguration());

        modelBuilder.ApplyConfiguration(
            new OrganizationConfiguration());

        modelBuilder.ApplyConfiguration(
            new PointTransactionConfiguration());

        modelBuilder.ApplyConfiguration(
            new RedemptionConfiguration());

        modelBuilder.ApplyConfiguration(
            new StayConfiguration());
    }
}
