namespace Shared.Tests;

using Shared.Application.Messaging;
using Shared.Domain;
using Shared.Infrastructure.Messaging;
using Xunit;

[Trait("Category", "Unit")]
public sealed class IntegrationEventEnvelopeFactoryTests
{
    [Fact]
    public void Create_normalizes_subject_parts()
    {
        IntegrationEventEnvelope envelope = IntegrationEventEnvelopeFactory.Create(
            " Auth ",
            new TestIntegrationEvent(" Member-Registered ", 1, " tenant-a "),
            " GMA ");

        Assert.Equal("gma.auth.member-registered.v1", envelope.Subject);
        Assert.Equal("tenant-a", envelope.TenantId);
    }

    [Fact]
    public void Create_rejects_blank_event_name()
    {
        Assert.Throws<ArgumentException>(() => IntegrationEventEnvelopeFactory.Create(
            "auth",
            new TestIntegrationEvent(" ", 1, "tenant-a")));
    }

    [Fact]
    public void Create_rejects_invalid_subject_parts()
    {
        Assert.Throws<ArgumentException>(() => IntegrationEventEnvelopeFactory.Create(
            "auth.module",
            new TestIntegrationEvent("member-registered", 1, "tenant-a")));
        Assert.Throws<ArgumentException>(() => IntegrationEventEnvelopeFactory.Create(
            "auth",
            new TestIntegrationEvent("member registered", 1, "tenant-a")));
        Assert.Throws<ArgumentException>(() => IntegrationEventEnvelopeFactory.Create(
            "auth",
            new TestIntegrationEvent("member-registered", 1, "tenant-a"),
            "gma.local"));
    }

    [Fact]
    public void Create_rejects_empty_event_id()
    {
        Assert.Throws<ArgumentException>(() => IntegrationEventEnvelopeFactory.Create(
            "auth",
            new TestIntegrationEvent("member-registered", 1, "tenant-a", Guid.Empty)));
    }

    [Fact]
    public void Create_rejects_non_positive_event_version()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => IntegrationEventEnvelopeFactory.Create(
            "auth",
            new TestIntegrationEvent("member-registered", 0, "tenant-a")));
    }

    [Fact]
    public void Create_rejects_invalid_tenant_id()
    {
        Assert.Throws<ArgumentException>(() => IntegrationEventEnvelopeFactory.Create(
            "auth",
            new TestIntegrationEvent("member-registered", 1, new string('x', TenantIds.MaxLength + 1))));
    }

    private sealed record TestIntegrationEvent(
        string EventName,
        int Version,
        string TenantId,
        Guid? Id = null) : IIntegrationEvent
    {
        public Guid EventId { get; } = Id ?? Guid.Parse("d1d19ab1-a1e0-4a09-bba6-a4f268bb8f61");
        public DateTimeOffset OccurredAtUtc { get; } = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    }
}
