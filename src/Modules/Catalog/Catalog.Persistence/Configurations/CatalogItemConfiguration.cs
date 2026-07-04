namespace Catalog.Persistence.Configurations;

using Catalog.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class CatalogItemConfiguration : IEntityTypeConfiguration<CatalogItem>
{
    public void Configure(EntityTypeBuilder<CatalogItem> builder)
    {
        builder.ToTable("items");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Sku).HasMaxLength(CatalogItem.SkuMaxLength).IsRequired();
        builder.Property(item => item.Name).HasMaxLength(CatalogItem.NameMaxLength).IsRequired();
        builder.Property(item => item.Price).HasPrecision(CatalogItem.PricePrecision, CatalogItem.PriceScale).IsRequired();
        builder.Property(item => item.Currency).HasMaxLength(CatalogItem.CurrencyLength).IsRequired();
        builder.Property(item => item.Status).HasConversion<int>().IsRequired();
        builder.HasIndex(item => new { item.TenantId, item.Sku }).IsUnique();
    }
}
