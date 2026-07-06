# 0012 Optional Notifications Module

## Status

Accepted.

## Context

The shared notification stack provides front-door live delivery through SSE and SignalR. That is useful for connected clients, but many products also need notification history, unread state, and a way for users to recover messages they missed while offline.

History should not live in `Shared.Notifications.Infrastructure`: storage is application state, and application state belongs to optional modules that own their schema, migrations, repositories, and APIs.

## Decision

Add an optional `Notifications` module:

- `Notifications.Contracts` owns public history DTOs and module metadata.
- `Notifications.Domain` owns the tenant-scoped `UserNotification` aggregate and read-state rules.
- `Notifications.Application` owns current-user list/get/mark-read commands and queries.
- `Notifications.Persistence` owns the `notifications` schema, provider-split migrations, repository, unit of work, inbox store, and a `IUserNotificationHistoryWriter` implementation.
- `Notifications.Api` owns current-user HTTP endpoints and durable history streaming under `/api/notifications`.
- `Notifications.AdminApi` owns tenant-wide admin history/list/get/streaming endpoints under `/api/admin/notifications`.
- The module also owns durable broadcast notifications for `tenant-users`, `tenant-admins`, `platform-users`, and `platform-admins`. Broadcasts are stored once with per-recipient read receipts, rather than fan-out rows for every possible recipient.

`Shared.Notifications` adds only the small `IUserNotificationHistoryWriter` contract. `Shared.Notifications.Infrastructure` remains the publisher/runtime coordinator. It calls registered history writers before attempting live delivery and fails open on history-writer failures.

`Notifications.Contracts` also owns `UserNotificationRequestedIntegrationEvent`. Producers that need guaranteed history creation reference `Notifications.Contracts`, write this event through their own outbox, and keep the physical subject producer-scoped, for example `gma.ordering.user-notification-requested.v1`. The Notifications module consumes explicit producer subscriptions through its own inbox. Producer bindings are host/example composition through `AddUserNotificationRequestSubscription(producerModule)`; the reusable module descriptor does not subscribe to Ordering, Catalog, or any other producer by default.

The module is not registered in default hosts. Applications compose it explicitly:

```csharp
builder.AddModule<NotificationsModule>();
```

## Consequences

The design keeps history optional and removable. Modules that emit best-effort live notifications continue to depend only on `Shared.Notifications`. Modules that need guaranteed history creation may additionally reference `Notifications.Contracts`; they still do not reference `Notifications.Application`, `Notifications.Domain`, `Notifications.Persistence`, `Notifications.Api`, or `Notifications.AdminApi`.

Composing the module gives users history and read/unread state for notification publish requests that reach the shared publisher. It does not make notification creation atomic with another module's source transaction, and it does not replace outbox/NATS for durable business facts.

For guaranteed notification creation, prefer the module-owned request event over the shared in-process queue. Additional producer modules must add explicit subscriptions; wildcard notification request consumption is a future shared messaging concern, not hidden module magic.

Broadcasts stay module-owned. They do not require references to Auth or Administration persistence; recipient ids are opaque, and platform broadcasts are selected explicitly by the Notifications repository rather than through tenant query filters. Read receipts are idempotent per recipient scope/kind/id, `read-all` is bounded, and payload JSON is normalized and capped at 32 KB for durable rows.
