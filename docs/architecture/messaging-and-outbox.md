# Messaging and Outbox

Messaging is behind abstractions. Domain and application code should never depend on NATS directly.

## Abstractions

- `IIntegrationEvent`
- `IntegrationEvent`
- `IntegrationEventEnvelope`
- `IOutboxWriter`
- `IOutboxWriterRegistry`
- `IOutboxStore`
- `IEventBus`

Public module integration events inherit `IntegrationEvent` from `Shared.Messaging`.
The base owns event id, tenant id, occurrence time, event name, and version validation. Module event types keep payload-specific fields and validation local to the owning `.Contracts` project.
This keeps the skeleton compatible with common event metadata practices without forcing a full CloudEvents envelope into module payload classes.

## Runtime Adapter

`Shared.Messaging.Infrastructure` registers EF outbox/inbox helpers, the outbox writer registry, outbox options, messaging metrics, the outbox publisher loop, and a fail-fast null event bus. It does not reference NATS and does not start the outbox publisher by itself.
`Shared.Messaging.Nats` owns the NATS JetStream event bus, consumer hosted service, NATS options, and low-level `AddNatsJetStreamMessaging()` / `AddNatsJetStreamConsumers()` composition hooks.
`Shared.Messaging.Nats.Aspire` owns Aspire NATS client composition for production-style HTTP hosts.

HTTP hosts that need real publishing opt in by referencing `Shared.Messaging.Nats.Aspire` and calling:

```csharp
builder.AddMessagingInfrastructure();
builder.AddConfiguredNatsJetStreamMessaging();
```

That call is a no-op unless `NatsJetStream:Enabled=true`. When enabled, it validates `NatsJetStream` settings, requires `ConnectionStrings:nats`, configures the Aspire NATS client from that connection string, calls into `Shared.Messaging.Nats`, registers the NATS event bus, and starts `OutboxPublisherService`.
When the host does not opt in, `IEventBus` remains a null adapter and module outbox rows stay local until a publisher is composed.
Tools and short-lived hosts can use shared infrastructure without accidentally draining outboxes.

Lower-level test or custom hosts can still reference `Shared.Messaging.Nats`, provide `INatsConnection` themselves, and call `AddNatsJetStreamMessaging()` directly, but production hosts should use the configured Aspire adapter so connection-string behavior stays consistent.
The low-level messaging methods compose `AddMessagingInfrastructure()` idempotently, and that composes `AddRuntimeInfrastructure()` for shared clocks and id generation without pulling in CQRS or domain-event dispatch.

The NATS JetStream adapter publishes each outbox row with the outbox message id as `NatsJSPubOpts.MsgId`.
If the broker accepted a message but the local outbox mark-processed step failed, a later retry may publish the same outbox row again. JetStream duplicate tracking then returns a duplicate ack instead of storing another message, and the adapter treats that ack as a successful idempotent publish.
Consumers must still keep inbox idempotency because delivery remains at-least-once.

## Subject Format

Integration event subjects use:

```text
{application-namespace}.{module}.{event}.v{version}
```

Example:

```text
gma.auth.member-registered.v1
```

`gma` is the default `ApplicationIdentity:Namespace` for this skeleton. Set a project-specific namespace such as `acme-orders` before creating production streams, durable consumers, cache keys, or dashboards. Module contracts should keep stable logical module/event/version names and render physical subjects through `IntegrationEventNaming` or the module's subject factory methods.

## Outbox Flow

```text
Command handler
  -> aggregate raises domain event
  -> unit of work dispatches domain event
  -> domain event handler resolves module outbox writer
  -> module outbox writer stores integration event
  -> EF Core commits aggregate and outbox message
  -> OutboxPublisherService claims pending messages
  -> IEventBus publishes envelope
  -> outbox message marked processed
```

## Outbox Options

```json
{
  "Outbox": {
    "BatchSize": 25,
    "PollIntervalMilliseconds": 5000,
    "LockDurationMilliseconds": 60000,
    "MaxAttempts": 10
  }
}
```

Outbox runtime values are validated at startup. `BatchSize`, `PollIntervalMilliseconds`, `LockDurationMilliseconds`, and `MaxAttempts` must be positive.

NATS stream options:

```json
{
  "NatsJetStream": {
    "Enabled": false
  }
}
```

`StreamName` is optional. When it is absent, infrastructure derives a stream name from `ApplicationIdentity:Namespace`, for example `gma` becomes `GMA_EVENTS` and `acme-orders` becomes `ACME_ORDERS_EVENTS`. Override `NatsJetStream:StreamName` only when an existing broker naming policy requires it. The skeleton intentionally accepts only ASCII letters, digits, `-`, and `_` for stream names. That follows the portable subset of [NATS JetStream naming guidance](https://docs.nats.io/nats-concepts/jetstream/streams): stream names must not contain whitespace, `.`, `*`, `>`, path separators, or non-printable characters.

## Claiming and Retry

The outbox supports:

- batch claims;
- worker ownership;
- lock expiration and reclaim;
- retry scheduling;
- max-attempt exhaustion;
- wrong-worker mark prevention.

Publisher failures are isolated at module-store and message granularity. If one module outbox store cannot claim rows, the publisher logs that store failure and continues with the other registered stores. If publishing one message fails, the publisher records the failed attempt for that row and leaves the rest of the batch eligible for normal processing. A publish operation that is canceled before host shutdown is treated as a failed attempt; host shutdown cancellation still stops the background service without rewriting outbox state.

Broker-side publish de-duplication is a defensive layer, not a replacement for outbox state. The local outbox row remains the source of retry truth, and consumer inbox tables remain the source of handler idempotency truth.

Outbox metadata limits are declared by `OutboxMessage` and consumed by module EF mappings. Subject, event type, tenant id, worker id, and bounded failure metadata should fail or truncate in shared infrastructure before a provider-specific `SaveChanges` path can fail late.

## Inbox And Consumers

NATS consumers are optional and documented in [Messaging Consumers](messaging-consumers.md).

Each consuming module owns an inbox table in its schema. The shared consumer runtime acknowledges a NATS message only after the handler effect and inbox processed marker commit.

## Messaging Guidelines

- Publish integration events only through outbox.
- Use `IOutboxWriterRegistry` from application handlers; do not inject a bare `IOutboxWriter`.
- Keep event names stable.
- Version events explicitly.
- Include tenant id when the event is tenant-scoped.
- Inherit public module events from `IntegrationEvent`; do not re-declare event id, tenant id, occurrence time, event name, or version in every event.
- Prefer additive event changes.
- Do not expose internal domain entities as integration events.
- Consumers should reference producer `.Contracts` only and duplicate local read data when they need it for decisions.
