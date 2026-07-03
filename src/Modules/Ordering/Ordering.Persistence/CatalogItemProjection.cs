namespace Ordering.Persistence;

using Catalog.Contracts;
using Ordering.Domain.Aggregates;
using Shared.Domain;
using Shared.ErrorHandling;

public sealed class CatalogItemProjection
{
    private CatalogItemProjection() { }

    private CatalogItemProjection(
        Guid id,
        string tenantId,
        Guid catalogItemId,
        string sku,
        string name,
        decimal price,
        string currency,
        CatalogItemStatus status)
    {
        this.Id = id;
        this.TenantId = TenantIds.Normalize(tenantId);
        this.CatalogItemId = catalogItemId;
        this.Apply(sku, name, price, currency, status);
    }

    private CatalogItemProjection(
        Guid id,
        string tenantId,
        Guid catalogItemId)
    {
        this.Id = id;
        this.TenantId = TenantIds.Normalize(tenantId);
        this.CatalogItemId = catalogItemId;
        this.Sku = string.Empty;
        this.Name = string.Empty;
        this.Price = 0;
        this.Currency = string.Empty;
        this.Status = CatalogItemStatus.Discontinued;
    }

    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = string.Empty;
    public Guid CatalogItemId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public CatalogItemStatus Status { get; private set; }

    public static CatalogItemProjection Create(
        Guid id,
        string tenantId,
        Guid catalogItemId,
        string sku,
        string name,
        decimal price,
        string currency,
        CatalogItemStatus status) =>
        new(id, tenantId, catalogItemId, sku, name, price, currency, status);

    public static CatalogItemProjection CreateDiscontinuedPlaceholder(
        Guid id,
        string tenantId,
        Guid catalogItemId) =>
        new(id, tenantId, catalogItemId);

    public void Update(string sku, string name, decimal price, string currency, CatalogItemStatus status) =>
        this.Apply(sku, name, price, currency, status);

    public void MarkDiscontinued() => this.Status = CatalogItemStatus.Discontinued;

    private void Apply(string sku, string name, decimal price, string currency, CatalogItemStatus status)
    {
        Result validation = Order.ValidateCatalogSnapshot(
            this.CatalogItemId,
            sku,
            name,
            price,
            currency);

        if (validation.IsFailure)
        {
            throw new ArgumentException(validation.Error.Code, nameof(sku));
        }

        this.Sku = Order.NormalizeCatalogSku(sku);
        this.Name = Order.NormalizeCatalogItemName(name);
        this.Price = price;
        this.Currency = Order.NormalizeCurrency(currency);
        this.Status = NormalizeStatus(status);
    }

    private static CatalogItemStatus NormalizeStatus(CatalogItemStatus status) =>
        Enum.IsDefined(status) ? status : CatalogItemStatus.Unknown;
}
