namespace Catalog.Domain.Aggregates;

using Catalog.Domain.Errors;
using Catalog.Domain.Events;
using Shared.Domain;
using Shared.Domain.Models;
using Shared.ErrorHandling;

public sealed class CatalogItem : AggregateRoot<Guid>, ITenantScoped
{
    public const int SkuMaxLength = 64;
    public const int NameMaxLength = 256;
    public const int CurrencyLength = 3;
    public const int PricePrecision = 18;
    public const int PriceScale = 2;

    private CatalogItem() { }

    private CatalogItem(Guid id, string tenantId)
        : base(id) => this.TenantId = tenantId;

    public string TenantId { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = string.Empty;
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

        Result validation = Validate(tenantId, sku, name, price, currency);
        if (validation.IsFailure)
        {
            return Result.Failure<CatalogItem>(validation.Error);
        }

        CatalogItem item = new(id, TenantIds.Normalize(tenantId))
        {
            Sku = NormalizeSku(sku),
            Name = NormalizeName(name),
            Price = price,
            Currency = NormalizeCurrency(currency),
            CreatedAtUtc = nowUtc
        };

        item.RaiseDomainEvent(new CatalogItemCreatedDomainEvent(
            eventId,
            nowUtc,
            item.Id,
            item.TenantId,
            item.Sku,
            item.Name,
            item.Price,
            item.Currency));

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

        Result validation = Validate(this.TenantId, sku, name, price, currency);
        if (validation.IsFailure)
        {
            return validation;
        }

        this.Sku = NormalizeSku(sku);
        this.Name = NormalizeName(name);
        this.Price = price;
        this.Currency = NormalizeCurrency(currency);
        this.UpdatedAtUtc = nowUtc;

        this.RaiseDomainEvent(new CatalogItemUpdatedDomainEvent(
            eventId,
            nowUtc,
            this.Id,
            this.TenantId,
            this.Sku,
            this.Name,
            this.Price,
            this.Currency,
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
            this.Sku));

        return Result.Success();
    }

    public static string NormalizeSku(string? sku) =>
        string.IsNullOrWhiteSpace(sku) ? string.Empty : sku.Trim().ToUpperInvariant();

    private static string NormalizeName(string? name) =>
        string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim();

    private static string NormalizeCurrency(string? currency) =>
        string.IsNullOrWhiteSpace(currency) ? string.Empty : currency.Trim().ToUpperInvariant();

    private static Result Validate(string tenantId, string? sku, string? name, decimal price, string? currency)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return Result.Failure(CatalogDomainErrors.TenantRequired);
        }

        if (!TenantIds.TryNormalize(tenantId, out _))
        {
            return Result.Failure(CatalogDomainErrors.TenantInvalid);
        }

        string normalizedSku = NormalizeSku(sku);
        if (string.IsNullOrWhiteSpace(normalizedSku))
        {
            return Result.Failure(CatalogDomainErrors.SkuRequired);
        }

        if (normalizedSku.Length > SkuMaxLength)
        {
            return Result.Failure(CatalogDomainErrors.SkuTooLong);
        }

        string normalizedName = NormalizeName(name);
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return Result.Failure(CatalogDomainErrors.NameRequired);
        }

        if (normalizedName.Length > NameMaxLength)
        {
            return Result.Failure(CatalogDomainErrors.NameTooLong);
        }

        if (price <= 0)
        {
            return Result.Failure(CatalogDomainErrors.PriceMustBePositive);
        }

        if (!DecimalPrecision.Fits(price, PricePrecision, PriceScale))
        {
            return Result.Failure(CatalogDomainErrors.PriceNotSupported);
        }

        string normalizedCurrency = NormalizeCurrency(currency);
        if (string.IsNullOrWhiteSpace(normalizedCurrency))
        {
            return Result.Failure(CatalogDomainErrors.CurrencyRequired);
        }

        return normalizedCurrency.Length == CurrencyLength
            ? Result.Success()
            : Result.Failure(CatalogDomainErrors.CurrencyInvalid);
    }

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
}
