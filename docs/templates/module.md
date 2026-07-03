# <Module> Module

## Purpose

Describe what this module owns and what it intentionally does not own.

## Projects

```text
<Module>.Contracts
<Module>.Domain
<Module>.Application
<Module>.Infrastructure
<Module>.Persistence
<Module>.Persistence.SqlServerMigrations
<Module>.Persistence.PostgreSqlMigrations
<Module>.Api
<Module>.Admin.Contracts
<Module>.AdminCli
<Module>.AdminApi
```

Remove projects that do not exist.

## Public Contracts

List public request/response DTOs, enums, admin permission code strings, module metadata, and integration events.
Keep one public contract type per file in `<Module>.Contracts`.
Place public contract files in the standard folders:

- `Api/` for public request/response/DTO records.
- `Admin/` for admin-facing DTO records that remain backend-free.
- `Events/` for integration event payloads and subject constants.
- `Metadata/` for `<Module>ModuleMetadata`, `*PermissionCodes`, and `*ContractLimits`.
- `Types/` for public enum-like or code-list types.

Confirm the public contracts `.csproj` references only `Shared.Application` plus optional producer `.Contracts` projects, and has no package or framework references.
Admin permission code strings live here so `<Module>ModuleMetadata` can declare permissions without referencing admin-only framework packages.
When the module becomes compiled code, add its contract metadata descriptor to `tests/Architecture.Tests/Support/ArchitectureCatalog.cs`; architecture tests compare that catalog with every `<Module>ModuleMetadata.Descriptor`.
If this module consumes another module's contracts, do not expose producer DTOs or enums from this module's public contracts. Duplicate the scalar/read-model fields owned by this module.

## Admin Contracts

List typed `AdminPermission` constants in `<Module>.Admin.Contracts`, if the module has admin CLI or admin API surfaces. These typed wrappers should point at the code strings from `<Module>.Contracts`. Confirm that public `<Module>.Contracts` does not reference `Shared.Administration`.
Place typed permission wrappers in `Permissions/` and operation-name constants in `Operations/`.
Confirm the admin contracts `.csproj` references only `Shared.Administration` and the owning public `<Module>.Contracts` project, with no package or framework references.

The module scaffolder seeds admin-capable modules with one `<module>.manage` permission in public metadata and typed admin contracts. Replace or split that seed permission once the module has real resources and operations; keep `*PermissionCodes` constants and `ModuleDescriptor.Permissions` in sync.

## Endpoints

Base path:

```text
/api/<module>
```

Endpoints:

- `METHOD /path`

State whether endpoints require tenant context, authorization, or both.
Confirm the public API `.csproj` has no package references, only an optional `Microsoft.AspNetCore.App` framework reference, shared API/application references as needed, and owning module contracts/application/persistence/infrastructure adapters.

## Admin Commands

List optional CLI commands, required permissions, tenant requirements, destructive confirmation, and secret-handling rules.
Confirm the admin CLI `.csproj` uses `System.CommandLine` as its only package reference, has no framework references, and references only shared administration/application contracts as needed plus owning module contracts/application/persistence/infrastructure adapters.

| Command | Permission | Tenant required | Destructive |
| --- | --- | --- | --- |
| `<module> <resource> <action>` | `<module>.<resource>.<action>` | yes/no | yes/no |

## Admin API

List optional admin HTTP routes, required permissions, tenant requirements, destructive confirmation, and secret-handling rules.
Confirm the admin API `.csproj` has no package references, only an optional `Microsoft.AspNetCore.App` framework reference, shared API/admin API/application references as needed, and owning module admin contracts/contracts/application/persistence/infrastructure adapters.

| Method | Route | Permission | Tenant required | Destructive |
| --- | --- | --- | --- | --- |
| `GET` | `/api/admin/<module>/<resource>` | `<module>.<resource>.read` | yes/no | no |

## Domain Model

List aggregates, entities, value objects, domain events, and core invariants.
Confirm domain events inherit `DomainEvent` or `TenantDomainEvent` from `Shared.Domain` so event id, occurrence time, and tenant id rules stay centralized.
Confirm the domain project references only shared domain/error primitives and does not reference contracts, application, persistence, infrastructure, HTTP, admin, EF, or host abstractions.
Confirm the domain `.csproj` has no package or framework references unless a future ADR explicitly expands the domain dependency model.

## Application Layer

List commands, queries, handlers, validators, domain event handlers, and integration event handlers.
State which commands implement `ITransactionalCommand<TResponse>` and which commands intentionally remain plain `ICommand<TResponse>`.
Keep one handler class per file under `<Module>.Application/Handlers`, including command handlers, query handlers, domain-event projectors, and integration-event handlers.
Confirm the application project does not reference module adapters or front doors such as `.Persistence`, `.Infrastructure`, `.Api`, `.AdminCli`, or `.AdminApi`.
Confirm the application `.csproj` references only shared abstractions, its own contracts/domain projects, optional producer `.Contracts` projects, and small Microsoft extension abstraction packages.
Do not reference `Shared.Administration` from feature module application projects; keep admin framework usage in `.AdminCli`/`.AdminApi` front doors. `Administration.Application` is the owner-side exception.
Confirm that handlers use `ISystemClock` and `IIdGenerator` instead of direct system time or ID generation.
Confirm that application DI registration extends `IServiceCollection`, not `IHostApplicationBuilder`.
Confirm that application DI extension methods reject null receivers and call `AddApplicationServicesFromAssembly(typeof(DependencyInjection).Assembly)` for CQRS handlers, validators, and domain-event handlers.
Confirm that integration-event handlers are registered explicitly with `AddIntegrationEventHandler<TEvent,THandler>(...)` so subject names and stable handler names remain public contracts.

## Infrastructure

Describe adapters and external systems.

## Persistence

Describe schema, DbContext, migrations, repositories, local projections, and outbox/inbox tables.
State the module name used by `IUnitOfWork`, `IOutboxWriter`, `IOutboxStore`, and `IInboxStore`.
For EF-backed modules that raise domain events, confirm the module UoW inherits `EfDomainEventUnitOfWork<TDbContext>` instead of duplicating domain-event dispatch/save/clear logic.
Confirm the persistence `.csproj` stays a provider adapter: EF SQL Server/PostgreSQL plus hosting packages only, shared application/domain/infrastructure primitives, the owning contracts/application/domain projects, and optional producer `.Contracts` projects for local projections.
If the module has a persistence project, keep both SQL Server and PostgreSQL migration projects unless a future ADR explicitly narrows provider support.
Confirm that EF design-time factories and `Microsoft.EntityFrameworkCore.Design` live only in provider-specific migration projects.
Confirm that provider-specific migration projects reference only the owning `<Module>.Persistence` project.
Confirm that persistence DI extension methods reject null receivers, call `AddPersistenceOptions(builder.Configuration)`, use `TryAddModuleDbContext`, and register UoW/outbox/inbox services through `TryAddEnumerable`.
Confirm that persisted enum numeric values are stable and that public contract/domain-state enums use `Unknown = 0`.

## Integration Events

| Event | Subject | Version | Tenant-scoped |
| --- | --- | --- | --- |
| `<EventName>` | `gma.<module>.<event>.v1` | `1` | yes/no |

Confirm public integration event contracts inherit `IntegrationEvent` from `Shared.Application.Messaging`, pass the stable event name/version to the base constructor, and keep only payload-specific validation in the module contract type.

## Inbound Subscriptions

List producer contracts consumed by this module. State handler name, subject, local projection, idempotency key, and whether tenant context is set from the event.

| Producer | Event | Subject | Handler | Local state updated |
| --- | --- | --- | --- | --- |
| `<Producer>` | `<EventName>` | `gma.<producer>.<event>.v1` | `<stable-handler-name>` | `<projection/table>` |

Use lowercase kebab-case for module, event, and handler-name segments. Subjects must follow `gma.<module>.<event>.v<version>`.
Confirm that the module references producer `.Contracts` only.
Confirm that consumed producer enum/status values are validated before they affect local decisions.

## Tests

List unit, architecture, and integration test coverage.

## Observability

List module meters, instruments, activity sources, structured log properties, and any alert-worthy conditions. Confirm that metric tags are bounded.

## Caching

List explicit cache-aside reads, logical keys, tags, TTL policy, and the commands/domain events that enqueue invalidation. State why each cached value is non-authoritative and whether it is tenant or global scoped. Confirm that the module references only `Shared.Application` caching contracts.

## Extension Points

List likely future changes and the intended extension mechanism.
