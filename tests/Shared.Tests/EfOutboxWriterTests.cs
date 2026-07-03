namespace Shared.Tests;

using Microsoft.EntityFrameworkCore;
using Shared.Application.Messaging;
using Shared.Application.Time;
using Shared.Infrastructure.Messaging;
using Xunit;

[Trait("Category", "Unit")]
public sealed class EfOutboxWriterTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 2, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_normalizes_module_name_through_integration_contracts()
    {
        using TestDbContext dbContext = CreateDbContext();

        TestOutboxWriter writer = new(dbContext, " Catalog ");

        Assert.Equal("catalog", writer.ModuleName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("catalog.module")]
    [InlineData("Catalog Module")]
    public void Constructor_rejects_invalid_module_name(string moduleName)
    {
        using TestDbContext dbContext = CreateDbContext();

        Assert.Throws<ArgumentException>(() => new TestOutboxWriter(dbContext, moduleName));
    }

    [Fact]
    public async Task Enqueue_tracks_outbox_message_without_saving()
    {
        using TestDbContext dbContext = CreateDbContext();
        TestOutboxWriter writer = new(dbContext, "catalog");
        TestIntegrationEvent integrationEvent = new(
            Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            "item-created",
            1,
            " tenant-a ",
            new DateTimeOffset(2026, 7, 2, 11, 30, 0, TimeSpan.Zero));

        await writer.EnqueueAsync(integrationEvent, CancellationToken.None);

        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<OutboxMessage> entry =
            Assert.Single(dbContext.ChangeTracker.Entries<OutboxMessage>());
        Assert.Equal(EntityState.Added, entry.State);
        OutboxMessage message = entry.Entity;
        Assert.Equal(integrationEvent.EventId, message.Id);
        Assert.Equal("gma.catalog.item-created.v1", message.Subject);
        Assert.Equal(typeof(TestIntegrationEvent).FullName, message.EventType);
        Assert.Equal(1, message.Version);
        Assert.Equal("tenant-a", message.TenantId);
        Assert.Equal(integrationEvent.OccurredAtUtc, message.OccurredAtUtc);
        Assert.Equal(Now, message.CreatedAtUtc);
        Assert.Contains("\"eventName\":\"item-created\"", message.Payload, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Enqueue_rejects_null_event()
    {
        using TestDbContext dbContext = CreateDbContext();
        TestOutboxWriter writer = new(dbContext, "catalog");

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            writer.EnqueueAsync<TestIntegrationEvent>(null!, CancellationToken.None));
    }

    [Fact]
    public async Task Enqueue_honors_cancellation_before_tracking_message()
    {
        using TestDbContext dbContext = CreateDbContext();
        TestOutboxWriter writer = new(dbContext, "catalog");
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            writer.EnqueueAsync(
                new TestIntegrationEvent(
                    Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                    "item-created",
                    1,
                    "tenant-a",
                    Now),
                cancellation.Token));

        Assert.Empty(dbContext.ChangeTracker.Entries<OutboxMessage>());
    }

    private static TestDbContext CreateDbContext()
    {
        DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new TestDbContext(options);
    }

    private sealed class TestOutboxWriter(TestDbContext dbContext, string moduleName)
        : EfOutboxWriter<TestDbContext>(dbContext, new TestClock(), moduleName);

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<OutboxMessage> OutboxMessages => this.Set<OutboxMessage>();
    }

    private sealed class TestClock : ISystemClock
    {
        public DateTimeOffset UtcNow => Now;
    }

    private sealed record TestIntegrationEvent(
        Guid EventId,
        string EventName,
        int Version,
        string TenantId,
        DateTimeOffset OccurredAtUtc) : IIntegrationEvent;
}
