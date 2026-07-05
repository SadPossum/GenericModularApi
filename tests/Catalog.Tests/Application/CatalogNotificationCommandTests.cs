namespace Catalog.Tests;

using Catalog.Application;
using Catalog.Application.Commands;
using Catalog.Application.Ports;
using Catalog.Contracts;
using Catalog.Domain.Aggregates;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Contracts;
using Shared.Caching;
using Shared.Cqrs;
using Shared.Messaging;
using Shared.Results;
using Shared.Runtime.Identity;
using Shared.Runtime.Time;
using Xunit;

[Trait("Category", "Unit")]
public sealed class CatalogNotificationCommandTests
{
    [Fact]
    public async Task Update_command_enqueues_notifications_module_event_when_recipient_is_supplied()
    {
        CatalogItem item = CreateItem();
        RecordingOutboxWriter outbox = new(CatalogModuleMetadata.Name);
        await using ServiceProvider provider = BuildProvider(item, outbox);
        ICommandHandler<UpdateCatalogItemCommand, CatalogItemDto> handler =
            provider.GetRequiredService<ICommandHandler<UpdateCatalogItemCommand, CatalogItemDto>>();

        Result<CatalogItemDto> result = await handler.HandleAsync(
            new UpdateCatalogItemCommand(item.Id, "sku-2", "Updated item", 12m, "USD", " user-a "),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        UserNotificationRequestedIntegrationEvent notification =
            Assert.IsType<UserNotificationRequestedIntegrationEvent>(Assert.Single(outbox.Events));
        Assert.Equal("tenant-a", notification.TenantId);
        Assert.Equal("user-a", notification.UserId);
        Assert.Equal(CatalogModuleMetadata.Name, notification.SourceModule);
        Assert.Equal(CatalogNotificationNames.ItemUpdated, notification.NotificationName);
        Assert.Equal("Catalog item updated", notification.Title);
        Assert.Contains("SKU-2", notification.PayloadJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Update_command_keeps_notification_request_optional()
    {
        CatalogItem item = CreateItem();
        RecordingOutboxWriter outbox = new(CatalogModuleMetadata.Name);
        await using ServiceProvider provider = BuildProvider(item, outbox);
        ICommandHandler<UpdateCatalogItemCommand, CatalogItemDto> handler =
            provider.GetRequiredService<ICommandHandler<UpdateCatalogItemCommand, CatalogItemDto>>();

        Result<CatalogItemDto> result = await handler.HandleAsync(
            new UpdateCatalogItemCommand(item.Id, "sku-2", "Updated item", 12m, "USD"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(outbox.Events);
    }

    private static ServiceProvider BuildProvider(CatalogItem item, RecordingOutboxWriter outbox)
    {
        ServiceCollection services = new();
        services.AddCatalogApplication();
        services.AddSingleton<ICatalogItemRepository>(new RecordingCatalogItemRepository(item));
        services.AddSingleton<ISystemClock>(new FixedClock());
        services.AddSingleton<IIdGenerator>(new FixedIdGenerator());
        services.AddSingleton<ICacheInvalidationQueue, RecordingCacheInvalidationQueue>();
        services.AddSingleton<IOutboxWriterRegistry>(new RecordingOutboxWriterRegistry(outbox));

        return services.BuildServiceProvider();
    }

    private static CatalogItem CreateItem() =>
        CatalogItem.Create(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "tenant-a",
            "SKU-1",
            "Catalog item",
            10m,
            "USD",
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)).Value;

    private sealed class RecordingCatalogItemRepository(CatalogItem item) : ICatalogItemRepository
    {
        public Task AddAsync(CatalogItem item, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<CatalogItem?> GetAsync(Guid itemId, CancellationToken cancellationToken) =>
            Task.FromResult(itemId == item.Id ? item : null);

        public Task<bool> SkuExistsAsync(string sku, Guid? excludingItemId, CancellationToken cancellationToken) =>
            Task.FromResult(false);
    }

    private sealed class FixedClock : ISystemClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 1, 2, 0, 0, 0, TimeSpan.Zero);
    }

    private sealed class FixedIdGenerator : IIdGenerator
    {
        public Guid NewId() => Guid.Parse("33333333-3333-3333-3333-333333333333");
    }

    private sealed class RecordingCacheInvalidationQueue : ICacheInvalidationQueue
    {
        public void Remove(CacheKey key) { }
        public void RemoveByTag(CacheTag tag) { }
    }

    private sealed class RecordingOutboxWriterRegistry(RecordingOutboxWriter writer) : IOutboxWriterRegistry
    {
        public IOutboxWriter GetRequired(string moduleName)
        {
            Assert.Equal(writer.ModuleName, moduleName);
            return writer;
        }
    }

    private sealed class RecordingOutboxWriter(string moduleName) : IOutboxWriter
    {
        public string ModuleName { get; } = moduleName;
        public List<IIntegrationEvent> Events { get; } = [];

        public Task EnqueueAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken)
            where TEvent : IIntegrationEvent
        {
            this.Events.Add(integrationEvent);
            return Task.CompletedTask;
        }
    }
}
