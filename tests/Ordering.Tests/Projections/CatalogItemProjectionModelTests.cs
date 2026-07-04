namespace Ordering.Tests;

using Shared.Naming;
using Catalog.Contracts;
using Ordering.Application.Ports;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Errors;
using Ordering.Persistence;
using Xunit;

[Trait("Category", "Unit")]
public sealed class CatalogItemProjectionModelTests
{
    private static readonly Guid ItemId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    [Fact]
    public void Write_model_normalizes_tenant_and_catalog_snapshot_data()
    {
        CatalogItemProjectionWriteModel model = new(
            " tenant-a ",
            ItemId,
            " sku-1 ",
            " Projection item ",
            10m,
            " usd ",
            CatalogItemStatus.Active);

        Assert.Equal("tenant-a", model.TenantId);
        Assert.Equal(ItemId, model.CatalogItemId);
        Assert.Equal("SKU-1", model.Sku);
        Assert.Equal("Projection item", model.Name);
        Assert.Equal(10m, model.Price);
        Assert.Equal("USD", model.Currency);
        Assert.Equal(CatalogItemStatus.Active, model.Status);
    }

    [Fact]
    public void Write_model_rejects_invalid_tenant_and_catalog_snapshot_data()
    {
        Assert.Throws<ArgumentException>(() => CreateWriteModel(tenantId: " "));
        Assert.Throws<ArgumentException>(() => CreateWriteModel(tenantId: new string('x', TenantIds.MaxLength + 1)));
        Assert.Throws<ArgumentException>(() => CreateWriteModel(catalogItemId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => CreateWriteModel(sku: " "));
        Assert.Throws<ArgumentException>(() => CreateWriteModel(sku: new string('x', Order.CatalogSkuMaxLength + 1)));
        Assert.Throws<ArgumentException>(() => CreateWriteModel(name: " "));
        Assert.Throws<ArgumentException>(() => CreateWriteModel(name: new string('x', Order.CatalogItemNameMaxLength + 1)));
        Assert.Throws<ArgumentException>(() => CreateWriteModel(price: 0));
        Assert.Throws<ArgumentException>(() => CreateWriteModel(price: 10.123m));
        Assert.Throws<ArgumentException>(() => CreateWriteModel(currency: "US"));
    }

    [Fact]
    public void Write_model_maps_undefined_status_to_unknown()
    {
        CatalogItemProjectionWriteModel model = CreateWriteModel(status: (CatalogItemStatus)999);

        Assert.Equal(CatalogItemStatus.Unknown, model.Status);
    }

    [Fact]
    public void Snapshot_normalizes_observed_data_but_preserves_invalid_business_state_for_callers()
    {
        CatalogItemProjectionSnapshot snapshot = new(
            ItemId,
            " sku-1 ",
            " Projection item ",
            0,
            " usd ",
            (CatalogItemStatus)999);

        Assert.Equal("SKU-1", snapshot.Sku);
        Assert.Equal("Projection item", snapshot.Name);
        Assert.Equal(0, snapshot.Price);
        Assert.Equal("USD", snapshot.Currency);
        Assert.Equal(CatalogItemStatus.Unknown, snapshot.Status);
    }

    [Fact]
    public void Snapshot_rejects_missing_catalog_item_id()
    {
        Assert.Throws<ArgumentException>(() => new CatalogItemProjectionSnapshot(
            Guid.Empty,
            "SKU-1",
            "Projection item",
            10m,
            "USD",
            CatalogItemStatus.Active));
    }

    [Fact]
    public void Persistence_projection_maps_undefined_status_to_unknown()
    {
        CatalogItemProjection projection = CatalogItemProjection.Create(
            Guid.Parse("bbbbbbbb-cccc-dddd-eeee-ffffffffffff"),
            "tenant-a",
            ItemId,
            "SKU-1",
            "Projection item",
            10m,
            "USD",
            (CatalogItemStatus)999);

        Assert.Equal(CatalogItemStatus.Unknown, projection.Status);
    }

    private static CatalogItemProjectionWriteModel CreateWriteModel(
        string tenantId = "tenant-a",
        Guid? catalogItemId = null,
        string sku = "SKU-1",
        string name = "Projection item",
        decimal price = 10m,
        string currency = "USD",
        CatalogItemStatus status = CatalogItemStatus.Active) =>
        new(
            tenantId,
            catalogItemId ?? ItemId,
            sku,
            name,
            price,
            currency,
            status);
}
