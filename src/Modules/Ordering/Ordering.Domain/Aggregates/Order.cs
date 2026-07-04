namespace Ordering.Domain.Aggregates;

using Shared.Naming;
using Ordering.Domain.Errors;
using Shared.Domain.Models;
using Shared.Numerics;
using Shared.Results;

public sealed class Order : TenantAggregateRoot<Guid>
{
    public const int CatalogSkuMaxLength = 64;
    public const int CatalogItemNameMaxLength = 256;
    public const int CurrencyLength = 3;
    public const int AmountPrecision = 18;
    public const int AmountScale = 2;

    private Order() { }

    private Order(Guid id, string tenantId)
        : base(id, tenantId)
    {
    }

    public Guid CatalogItemId { get; private set; }
    public string CatalogSku { get; private set; } = string.Empty;
    public string CatalogItemName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal Total { get; private set; }
    public OrderState Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static Result<Order> Create(
        Guid id,
        string tenantId,
        Guid catalogItemId,
        string catalogSku,
        string catalogItemName,
        decimal unitPrice,
        string currency,
        int quantity,
        DateTimeOffset nowUtc)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure<Order>(OrderingDomainErrors.OrderIdRequired);
        }

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return Result.Failure<Order>(OrderingDomainErrors.TenantRequired);
        }

        if (!TenantIds.TryNormalize(tenantId, out string? normalizedTenantId))
        {
            return Result.Failure<Order>(OrderingDomainErrors.TenantInvalid);
        }

        if (quantity <= 0)
        {
            return Result.Failure<Order>(OrderingDomainErrors.QuantityMustBePositive);
        }

        Result validation = ValidateCatalogSnapshot(catalogItemId, catalogSku, catalogItemName, unitPrice, currency);
        if (validation.IsFailure)
        {
            return Result.Failure<Order>(validation.Error);
        }

        if (!TryCalculateTotal(unitPrice, quantity, out decimal total))
        {
            return Result.Failure<Order>(OrderingDomainErrors.OrderTotalNotSupported);
        }

        Order order = new(id, normalizedTenantId)
        {
            CatalogItemId = catalogItemId,
            CatalogSku = NormalizeCatalogSku(catalogSku),
            CatalogItemName = NormalizeCatalogItemName(catalogItemName),
            UnitPrice = unitPrice,
            Currency = NormalizeCurrency(currency),
            Quantity = quantity,
            Total = total,
            Status = OrderState.Submitted,
            CreatedAtUtc = nowUtc
        };

        return Result.Success(order);
    }

    public static Result ValidateCatalogSnapshot(
        Guid catalogItemId,
        string? catalogSku,
        string? catalogItemName,
        decimal unitPrice,
        string? currency)
    {
        if (catalogItemId == Guid.Empty)
        {
            return Result.Failure(OrderingDomainErrors.CatalogItemRequired);
        }

        string normalizedSku = NormalizeCatalogSku(catalogSku);
        if (string.IsNullOrWhiteSpace(normalizedSku))
        {
            return Result.Failure(OrderingDomainErrors.CatalogSkuRequired);
        }

        if (normalizedSku.Length > CatalogSkuMaxLength)
        {
            return Result.Failure(OrderingDomainErrors.CatalogSkuTooLong);
        }

        string normalizedName = NormalizeCatalogItemName(catalogItemName);
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return Result.Failure(OrderingDomainErrors.CatalogItemNameRequired);
        }

        if (normalizedName.Length > CatalogItemNameMaxLength)
        {
            return Result.Failure(OrderingDomainErrors.CatalogItemNameTooLong);
        }

        if (unitPrice <= 0)
        {
            return Result.Failure(OrderingDomainErrors.CatalogItemPriceMustBePositive);
        }

        if (!DecimalPrecision.Fits(unitPrice, AmountPrecision, AmountScale))
        {
            return Result.Failure(OrderingDomainErrors.CatalogItemPriceNotSupported);
        }

        string normalizedCurrency = NormalizeCurrency(currency);
        if (normalizedCurrency.Length != CurrencyLength)
        {
            return Result.Failure(OrderingDomainErrors.CatalogItemCurrencyInvalid);
        }

        return Result.Success();
    }

    private static bool TryCalculateTotal(decimal unitPrice, int quantity, out decimal total)
    {
        try
        {
            total = unitPrice * quantity;
        }
        catch (OverflowException)
        {
            total = 0;
            return false;
        }

        return DecimalPrecision.Fits(total, AmountPrecision, AmountScale);
    }

    public static string NormalizeCatalogSku(string? catalogSku) =>
        string.IsNullOrWhiteSpace(catalogSku) ? string.Empty : catalogSku.Trim().ToUpperInvariant();

    public static string NormalizeCatalogItemName(string? catalogItemName) =>
        string.IsNullOrWhiteSpace(catalogItemName) ? string.Empty : catalogItemName.Trim();

    public static string NormalizeCurrency(string? currency) =>
        string.IsNullOrWhiteSpace(currency) ? string.Empty : currency.Trim().ToUpperInvariant();
}
