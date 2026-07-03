namespace Shared.Infrastructure.Messaging;

using Microsoft.EntityFrameworkCore;
using Shared.Application.Messaging;
using Shared.Application.Time;

public abstract class EfOutboxWriter<TDbContext>(
    TDbContext dbContext,
    ISystemClock clock,
    string moduleName)
    : IOutboxWriter
    where TDbContext : DbContext
{
    public string ModuleName { get; } = IntegrationEventNaming.NormalizeModuleName(moduleName);

    public Task EnqueueAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken)
        where TEvent : IIntegrationEvent
    {
        cancellationToken.ThrowIfCancellationRequested();

        IntegrationEventEnvelope envelope = IntegrationEventEnvelopeFactory.Create(
            this.ModuleName,
            integrationEvent);

        dbContext.Set<OutboxMessage>().Add(new OutboxMessage(
            envelope.EventId,
            envelope.Subject,
            envelope.EventType,
            envelope.Version,
            envelope.TenantId,
            envelope.OccurredAtUtc,
            envelope.Payload,
            clock.UtcNow));

        return Task.CompletedTask;
    }
}
