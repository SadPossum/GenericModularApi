namespace Ordering.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.Aggregates;
using Shared.Domain;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.TenantId).HasMaxLength(TenantIds.MaxLength).IsRequired();
        builder.Property(order => order.CatalogSku).HasMaxLength(Order.CatalogSkuMaxLength).IsRequired();
        builder.Property(order => order.CatalogItemName).HasMaxLength(Order.CatalogItemNameMaxLength).IsRequired();
        builder.Property(order => order.UnitPrice).HasPrecision(Order.AmountPrecision, Order.AmountScale).IsRequired();
        builder.Property(order => order.Total).HasPrecision(Order.AmountPrecision, Order.AmountScale).IsRequired();
        builder.Property(order => order.Currency).HasMaxLength(Order.CurrencyLength).IsRequired();
        builder.Property(order => order.Status).HasConversion<int>().IsRequired();
        builder.HasIndex(order => new { order.TenantId, order.CatalogItemId });
    }
}
