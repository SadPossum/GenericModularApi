namespace Shared.Tests;

using Shared.Application.Messaging;
using Shared.Domain;
using Xunit;

[Trait("Category", "Unit")]
public sealed class IntegrationEventContractGuardTests
{
    [Fact]
    public void Guard_normalizes_common_event_contract_metadata()
    {
        Guid eventId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        DateTimeOffset occurredAtUtc = new(2026, 7, 2, 12, 0, 0, TimeSpan.Zero);

        Assert.Equal(eventId, IntegrationEventContractGuards.RequireId(eventId, "eventId"));
        Assert.Equal("tenant-a", IntegrationEventContractGuards.NormalizeTenantId(" tenant-a ", "tenantId"));
        Assert.Equal(occurredAtUtc, IntegrationEventContractGuards.RequireOccurredAtUtc(occurredAtUtc, "occurredAtUtc"));
        Assert.Equal("text", IntegrationEventContractGuards.NormalizeRequiredText(" text ", 10, "value"));
    }

    [Fact]
    public void Guard_rejects_invalid_common_event_contract_metadata()
    {
        Assert.Throws<ArgumentException>(() => IntegrationEventContractGuards.RequireId(Guid.Empty, "eventId"));
        Assert.Throws<ArgumentException>(() => IntegrationEventContractGuards.NormalizeTenantId(" ", "tenantId"));
        Assert.Throws<ArgumentException>(() => IntegrationEventContractGuards.NormalizeTenantId(new string('x', TenantIds.MaxLength + 1), "tenantId"));
        Assert.Throws<ArgumentException>(() => IntegrationEventContractGuards.RequireOccurredAtUtc(default, "occurredAtUtc"));
        Assert.Throws<ArgumentException>(() => IntegrationEventContractGuards.NormalizeRequiredText(" ", 10, "value"));
        Assert.Throws<ArgumentException>(() => IntegrationEventContractGuards.NormalizeRequiredText("too-long", 3, "value"));
        Assert.Throws<ArgumentException>(() => IntegrationEventContractGuards.NormalizeRequiredText($"bad{char.MinValue}", 10, "value"));
    }

    [Fact]
    public void Guard_validates_numeric_contract_values()
    {
        Assert.Equal(10m, IntegrationEventContractGuards.RequirePositiveDecimal(10m, 18, 2, "price"));
        Assert.Equal(0, IntegrationEventContractGuards.RequireNonNegative(0, "count"));

        Assert.Throws<ArgumentException>(() => IntegrationEventContractGuards.RequirePositiveDecimal(0, 18, 2, "price"));
        Assert.Throws<ArgumentException>(() => IntegrationEventContractGuards.RequirePositiveDecimal(10.123m, 18, 2, "price"));
        Assert.Throws<ArgumentOutOfRangeException>(() => IntegrationEventContractGuards.RequireNonNegative(-1, "count"));
    }

    [Fact]
    public void Guard_maps_undefined_enum_values_to_unknown_default()
    {
        Assert.Equal(TestStatus.Active, IntegrationEventContractGuards.NormalizeDefinedOrUnknown(TestStatus.Active));
        Assert.Equal(TestStatus.Unknown, IntegrationEventContractGuards.NormalizeDefinedOrUnknown((TestStatus)999));
    }

    private enum TestStatus
    {
        Unknown = 0,
        Active = 1
    }
}
