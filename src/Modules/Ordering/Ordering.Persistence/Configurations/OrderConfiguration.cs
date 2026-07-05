namespace Ordering.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Errors;
using Ordering.Domain.ValueObjects;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.CatalogItemId)
            .HasField("catalogItemId")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .IsRequired();
        builder.Property(order => order.CatalogSku)
            .HasField("catalogSku")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasMaxLength(Order.CatalogSkuMaxLength)
            .IsRequired();
        builder.Property(order => order.CatalogItemName)
            .HasField("catalogItemName")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasMaxLength(Order.CatalogItemNameMaxLength)
            .IsRequired();
        builder.Property(order => order.UnitPrice)
            .HasField("unitPrice")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasPrecision(Order.AmountPrecision, Order.AmountScale)
            .IsRequired();
        builder.Property(order => order.Currency)
            .HasField("currency")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasMaxLength(Order.CurrencyLength)
            .IsRequired();
        builder.Property(order => order.Quantity)
            .HasConversion(quantity => quantity.Value, value => OrderQuantity.Create(value).Value)
            .HasColumnName("Quantity")
            .IsRequired();
        builder.Property(order => order.Total)
            .HasConversion(
                amount => amount.Value,
                value => OrderAmount.Create(
                    value,
                    OrderingDomainErrors.OrderTotalNotSupported,
                    OrderingDomainErrors.OrderTotalNotSupported).Value)
            .HasColumnName("Total")
            .HasPrecision(Order.AmountPrecision, Order.AmountScale)
            .IsRequired();
        builder.Property(order => order.Status).HasConversion<int>().IsRequired();
        builder.HasIndex(order => new { order.TenantId, order.CatalogItemId });
    }
}
