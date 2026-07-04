# Catalog Example Module

Catalog is a compiled optional example module. It is not registered in `Host.Api`, `Host.AdminCli`, or `Host.AdminApi` by default.

## Purpose

Catalog demonstrates stored tenant-scoped domain data, CQRS commands and queries, provider-split EF persistence, explicit cache-aside reads, admin CLI/API front doors, and integration events through outbox.

## Projects

```text
Catalog.Contracts
Catalog.Domain
Catalog.Application
Catalog.Persistence
Catalog.Persistence.SqlServerMigrations
Catalog.Persistence.PostgreSqlMigrations
Catalog.Api
Catalog.Admin.Contracts
Catalog.AdminCli
Catalog.AdminApi
```

`Catalog.Contracts` follows the standard contract folders: `Api/` for item DTO/list contracts, `Events/` for item integration events and subjects, `Exports/` for projection rebuild/export contracts, `Metadata/` for module metadata, limits, and permission code strings, and `Types/` for `CatalogItemStatus`.

## Domain

`CatalogItem` owns:

- tenant id;
- SKU;
- name;
- price;
- currency;
- status: active or discontinued.

Core rules:

- tenant id, SKU, name, price, and currency are required;
- price must be positive;
- price must fit the module's mapped decimal precision without rounding;
- SKU is normalized and unique per tenant;
- SKU, name, and three-letter currency limits are domain invariants before persistence;
- discontinued items cannot be discontinued again.

## Cache

Queries use explicit cache-aside through `IApplicationCache`.

Keys:

- `catalog:item:{itemId}`
- `catalog:items:{page}:{pageSize}`

Tags:

- `catalog:items`

Create/update/discontinue commands enqueue invalidation through `ICacheInvalidationQueue`. Cache data is non-authoritative and tenant-scoped.

## Admin Permissions

Permission code strings live in `Catalog.Contracts` for module metadata. Typed `AdminPermission` constants live in `Catalog.Admin.Contracts` for CLI/API front doors.

```text
catalog.items.read
catalog.items.create
catalog.items.update
catalog.items.discontinue
```

## Integration Events

| Event | Subject |
| --- | --- |
| `CatalogItemCreatedIntegrationEvent` | `{application-namespace}.catalog.item-created.v1` |
| `CatalogItemUpdatedIntegrationEvent` | `{application-namespace}.catalog.item-updated.v1` |
| `CatalogItemDiscontinuedIntegrationEvent` | `{application-namespace}.catalog.item-discontinued.v1` |

Events are written by domain-event handlers through the module outbox. The local default namespace is `gma`; `CatalogIntegrationSubjects` can render the same logical events under a configured application namespace.

## Projection Export

Catalog exposes a producer-owned export contract for rebuild/backfill scenarios:

- `CatalogItemProjectionExport` is the stable snapshot consumed by Ordering rebuilds.
- `ICatalogItemProjectionExportSource` is the contract-facing source port.
- `Catalog.Persistence` implements the port against Catalog's EF model.

Consumers reference `Catalog.Contracts` only. They do not reference Catalog domain, application, persistence, API, or admin projects for rebuilds.

## Compose Explicitly

Add project references from the host to the front doors you want, then register:

```csharp
builder.AddModule<CatalogModule>();
builder.AddAdminModule<CatalogAdminCliModule>();
builder.AddAdminApiModule<CatalogAdminApiModule>();
```

Only add the registrations to hosts that should expose Catalog.
