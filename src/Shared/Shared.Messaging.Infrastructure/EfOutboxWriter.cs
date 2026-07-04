namespace Shared.Messaging.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Messaging;
using Shared.Runtime;
using Shared.Runtime.Time;

public abstract class EfOutboxWriter<TDbContext>(
    TDbContext dbContext,
    ISystemClock clock,
    IOptions<ApplicationIdentityOptions> applicationIdentity,
    string moduleName)
    : IOutboxWriter
    where TDbContext : DbContext
{
    private readonly string subjectPrefix = applicationIdentity.Value.EffectiveNamespace;

    public string ModuleName { get; } = IntegrationEventNaming.NormalizeModuleName(moduleName);

    public Task EnqueueAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken)
        where TEvent : IIntegrationEvent
    {
        cancellationToken.ThrowIfCancellationRequested();

        IntegrationEventEnvelope envelope = IntegrationEventEnvelopeFactory.Create(
            this.ModuleName,
            integrationEvent,
            this.subjectPrefix);

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
