namespace Ordering.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.Aggregates;
using Shared.Domain;

internal sealed class CatalogItemProjectionConfiguration : IEntityTypeConfiguration<CatalogItemProjection>
{
    public void Configure(EntityTypeBuilder<CatalogItemProjection> builder)
    {
        builder.ToTable("catalog_item_projections");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.TenantId).HasMaxLength(TenantIds.MaxLength).IsRequired();
        builder.Property(item => item.Sku).HasMaxLength(Order.CatalogSkuMaxLength).IsRequired();
        builder.Property(item => item.Name).HasMaxLength(Order.CatalogItemNameMaxLength).IsRequired();
        builder.Property(item => item.Price).HasPrecision(Order.AmountPrecision, Order.AmountScale).IsRequired();
        builder.Property(item => item.Currency).HasMaxLength(Order.CurrencyLength).IsRequired();
        builder.Property(item => item.Status).HasConversion<int>().IsRequired();
        builder.HasIndex(item => new { item.TenantId, item.CatalogItemId }).IsUnique();
    }
}
