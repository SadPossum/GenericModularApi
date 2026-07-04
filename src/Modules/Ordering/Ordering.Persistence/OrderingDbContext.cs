namespace Ordering.Persistence;

using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Aggregates;
using Shared.Tenancy;
using Shared.Messaging.Infrastructure;

public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options, ITenantContext tenantContext)
    : DbContext(options)
{
    private readonly bool tenantFilteringEnabled = tenantContext.IsEnabled;
    private readonly string tenantId = tenantContext.TenantId ?? string.Empty;

    public DbSet<Order> Orders => this.Set<Order>();
    public DbSet<CatalogItemProjection> CatalogItemProjections => this.Set<CatalogItemProjection>();
    public DbSet<OrderingProjectionRebuildCheckpoint> ProjectionRebuildCheckpoints =>
        this.Set<OrderingProjectionRebuildCheckpoint>();
    public DbSet<InboxMessage> InboxMessages => this.Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(OrderingMigrations.Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderingDbContext).Assembly);

        modelBuilder.Entity<Order>()
            .HasQueryFilter("TenantFilter", order => !this.tenantFilteringEnabled || order.TenantId == this.tenantId);
        modelBuilder.Entity<CatalogItemProjection>()
            .HasQueryFilter("TenantFilter", item => !this.tenantFilteringEnabled || item.TenantId == this.tenantId);
        modelBuilder.Entity<OrderingProjectionRebuildCheckpoint>()
            .HasQueryFilter("TenantFilter", checkpoint => !this.tenantFilteringEnabled || checkpoint.TenantId == this.tenantId);
    }
}
