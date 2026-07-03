# <EventName> Integration Event

## Purpose

Describe why this event exists and who is expected to consume it.

## Subject

```text
gma.<module>.<event>.v<version>
```

## Version

`1`

## Owner

`<Module>`

## Payload

```json
{
  "eventId": "guid",
  "occurredAtUtc": "timestamp",
  "tenantId": "string"
}
```

## Semantics

Describe what fact the event represents.

## Compatibility Rules

- Prefer additive changes.
- Do not rename existing fields.
- Do not change field meaning.
- Create a new version for breaking changes.

## Producers

- `<Module>.Application`

## Consumers

- `<ConsumerModule>` handles this event with `<handler-name>` and stores idempotency in `<schema>.inbox_messages`.

If no consumer exists yet, write `No consumers yet` and include the intended compatibility promise or reason the event is currently producer-only. Do not leave this section as unknown.

For each consumer, document:

- stable lowercase kebab-case handler name;
- durable consumer name shape: `gma-<environment>-<consumer-module>-<handler-name>`;
- inbox table/schema;
- local projection/table updated;
- idempotency key;
- retry behavior;
- compatibility expectation.

## Tests

List tests proving the event is written to outbox, published, consumed idempotently, and projected by at least one consumer when applicable.
