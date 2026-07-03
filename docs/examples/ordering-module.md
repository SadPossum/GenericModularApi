# Ordering Example Module

Ordering is a compiled optional example module. It is not registered in `Host.Api`, `Host.AdminCli`, or `Host.AdminApi` by default.

## Purpose

Ordering demonstrates consuming another module's integration events without referencing that module's internals.

Ordering owns:

- orders;
- local catalog item projections used for order decisions;
- an inbox table for idempotent event processing.

## Projects

```text
Ordering.Contracts
Ordering.Domain
Ordering.Application
Ordering.Persistence
Ordering.Persistence.SqlServerMigrations
Ordering.Persistence.PostgreSqlMigrations
```

## Catalog Dependency

Ordering references `Catalog.Contracts` only.

It consumes:

- `CatalogItemCreatedIntegrationEvent`
- `CatalogItemUpdatedIntegrationEvent`
- `CatalogItemDiscontinuedIntegrationEvent`

It does not reference Catalog domain, application, persistence, API, or admin projects.

## Projection

`CatalogItemProjection` stores duplicated local data:

- catalog item id;
- SKU;
- name;
- price;
- currency;
- status.

`Order` stores the catalog item id as a scalar external id. There is no database foreign key to Catalog.

## Order Rule

Creating an order requires a known active catalog item projection. Unknown or discontinued catalog items are rejected.
Ordering validates duplicated projection data before creating an order: SKU and name must fit local persistence limits, currency must be a three-letter code, and price must be positive and fit mapped decimal precision without rounding. Order totals must also fit mapped decimal precision. This keeps copied data explicit and prevents a bad producer event or manual projection repair from becoming a late EF failure.

## Consumers

Ordering registers handlers with:

```csharp
builder.Services.AddIntegrationEventHandler<CatalogItemCreatedIntegrationEvent, CatalogItemCreatedProjectionHandler>(
    OrderingModuleMetadata.Name,
    CatalogIntegrationSubjects.ItemCreated,
    OrderingModuleMetadata.CatalogItemCreatedProjectionHandlerName);
```

The real application registration uses `OrderingModuleMetadata` handler constants for all three Catalog subscriptions. The NATS runtime invokes the handler. `OrderingInboxStore` records processing in the `ordering` schema.
