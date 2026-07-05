namespace Catalog.Domain.Aggregates;

using Shared.Naming;
using Catalog.Domain.Errors;
using Catalog.Domain.Events;
using Catalog.Domain.ValueObjects;
using Shared.Domain.Models;
using Shared.Results;

public sealed class CatalogItem : TenantAggregateRoot<Guid>
{
    public const int SkuMaxLength = 64;
    public const int NameMaxLength = 256;
    public const int CurrencyLength = 3;
    public const int PricePrecision = 18;
    public const int PriceScale = 2;

    private CatalogItem() { }

    private CatalogItem(Guid id, string tenantId)
        : base(id, tenantId)
    {
    }

    public CatalogSku Sku { get; private set; }
    public CatalogItemName Name { get; private set; }
    public CatalogPrice Price { get; private set; }
    public CurrencyCode Currency { get; private set; }
    public CatalogItemState Status { get; private set; } = CatalogItemState.Active;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public DateTimeOffset? DiscontinuedAtUtc { get; private set; }

    public static Result<CatalogItem> Create(
        Guid id,
        string tenantId,
        string sku,
        string name,
        decimal price,
        string currency,
        Guid eventId,
        DateTimeOffset nowUtc)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure<CatalogItem>(CatalogDomainErrors.ItemIdRequired);
        }

        if (eventId == Guid.Empty)
        {
            return Result.Failure<CatalogItem>(CatalogDomainErrors.DomainEventIdRequired);
        }

        Result<CatalogItemValues> values = CatalogItemValues.Create(tenantId, sku, name, price, currency);
        if (values.IsFailure)
        {
            return Result.Failure<CatalogItem>(values.Error);
        }

        CatalogItem item = new(id, values.Value.TenantId)
        {
            Sku = values.Value.Sku,
            Name = values.Value.Name,
            Price = values.Value.Price,
            Currency = values.Value.Currency,
            CreatedAtUtc = nowUtc
        };

        item.RaiseDomainEvent(new CatalogItemCreatedDomainEvent(
            eventId,
            nowUtc,
            item.Id,
            item.TenantId,
            item.Sku.Value,
            item.Name.Value,
            item.Price.Value,
            item.Currency.Value));

        return Result.Success(item);
    }

    public Result Update(
        string sku,
        string name,
        decimal price,
        string currency,
        Guid eventId,
        DateTimeOffset nowUtc)
    {
        Result statusResult = this.EnsureKnownStatus();
        if (statusResult.IsFailure)
        {
            return statusResult;
        }

        if (eventId == Guid.Empty)
        {
            return Result.Failure(CatalogDomainErrors.DomainEventIdRequired);
        }

        Result<CatalogItemValues> values = CatalogItemValues.Create(this.TenantId, sku, name, price, currency);
        if (values.IsFailure)
        {
            return Result.Failure(values.Error);
        }

        this.Sku = values.Value.Sku;
        this.Name = values.Value.Name;
        this.Price = values.Value.Price;
        this.Currency = values.Value.Currency;
        this.UpdatedAtUtc = nowUtc;

        this.RaiseDomainEvent(new CatalogItemUpdatedDomainEvent(
            eventId,
            nowUtc,
            this.Id,
            this.TenantId,
            this.Sku.Value,
            this.Name.Value,
            this.Price.Value,
            this.Currency.Value,
            this.Status));

        return Result.Success();
    }

    public Result Discontinue(Guid eventId, DateTimeOffset nowUtc)
    {
        Result statusResult = this.EnsureCanDiscontinue();
        if (statusResult.IsFailure)
        {
            return statusResult;
        }

        if (eventId == Guid.Empty)
        {
            return Result.Failure(CatalogDomainErrors.DomainEventIdRequired);
        }

        this.Status = CatalogItemState.Discontinued;
        this.DiscontinuedAtUtc = nowUtc;
        this.UpdatedAtUtc = nowUtc;

        this.RaiseDomainEvent(new CatalogItemDiscontinuedDomainEvent(
            eventId,
            nowUtc,
            this.Id,
            this.TenantId,
            this.Sku.Value));

        return Result.Success();
    }

    public static string NormalizeSku(string? sku) =>
        CatalogSku.Normalize(sku);

    private Result EnsureCanDiscontinue() =>
        this.Status switch
        {
            CatalogItemState.Active => Result.Success(),
            CatalogItemState.Discontinued => Result.Failure(CatalogDomainErrors.ItemAlreadyDiscontinued),
            _ => Result.Failure(CatalogDomainErrors.ItemStatusUnknown)
        };

    private Result EnsureKnownStatus() =>
        this.Status is CatalogItemState.Active or CatalogItemState.Discontinued
            ? Result.Success()
            : Result.Failure(CatalogDomainErrors.ItemStatusUnknown);

    private sealed record CatalogItemValues(
        string TenantId,
        CatalogSku Sku,
        CatalogItemName Name,
        CatalogPrice Price,
        CurrencyCode Currency)
    {
        public static Result<CatalogItemValues> Create(
            string tenantId,
            string? sku,
            string? name,
            decimal price,
            string? currency)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Result.Failure<CatalogItemValues>(CatalogDomainErrors.TenantRequired);
            }

            if (!TenantIds.TryNormalize(tenantId, out string? normalizedTenantId))
            {
                return Result.Failure<CatalogItemValues>(CatalogDomainErrors.TenantInvalid);
            }

            Result<CatalogSku> skuResult = CatalogSku.Create(sku);
            if (skuResult.IsFailure)
            {
                return Result.Failure<CatalogItemValues>(skuResult.Error);
            }

            Result<CatalogItemName> nameResult = CatalogItemName.Create(name);
            if (nameResult.IsFailure)
            {
                return Result.Failure<CatalogItemValues>(nameResult.Error);
            }

            Result<CatalogPrice> priceResult = CatalogPrice.Create(price);
            if (priceResult.IsFailure)
            {
                return Result.Failure<CatalogItemValues>(priceResult.Error);
            }

            Result<CurrencyCode> currencyResult = CurrencyCode.Create(currency);
            if (currencyResult.IsFailure)
            {
                return Result.Failure<CatalogItemValues>(currencyResult.Error);
            }

            return Result.Success(new CatalogItemValues(
                normalizedTenantId,
                skuResult.Value,
                nameResult.Value,
                priceResult.Value,
                currencyResult.Value));
        }
    }
}
