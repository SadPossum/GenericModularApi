# Module System

The module system is intentionally small.

```csharp
public interface IModule
{
    string Name { get; }
    void AddServices(IHostApplicationBuilder builder);
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
```

## Rules

- Modules are registered explicitly in `Host.Api`.
- Modules are not discovered by assembly scanning.
- A module owns its endpoints, application use cases, domain model, persistence, and contracts.
- A module should expose only contracts and integration events to other modules.
- A module must not require another module's internals.
- Persistent module commands implement `ITransactionalCommand<TResponse>`.
- Module `IUnitOfWork` and `IOutboxWriter` implementations declare `ModuleName`; shared infrastructure refuses ambiguous matches.
- Module projects must not reference concrete transport, cache-backend, or observability-exporter packages directly. Use shared abstractions plus the host-selected adapter.

## Shared Project Layers

The shared core is intentionally small:

- `Shared.Naming`, `Shared.Numerics`, `Shared.Observability`, and `Shared.Results` stay dependency-free.
- `Shared.Domain` owns aggregate/domain-event primitives and depends only on `Shared.Naming` for shared identifier syntax such as tenant ids and `Shared.Numerics` for reusable numeric validation.
- `Shared.Modules` owns module metadata primitives and references only `Shared.Naming`.
- `Shared.ModuleComposition` owns module profile, provided-feature, required-feature, required-module, and fail-fast composition validation primitives. It references only `Shared.Modules`, `Shared.Naming`, and hosting abstractions needed by composition roots.
- `Shared.Authorization` owns permission metadata descriptor extensions and references only `Shared.Modules` and `Shared.Naming`.
- `Shared.Caching` owns cache contracts, provider/options seams, adapter markers, cache descriptor metadata, and cache composition feature ids. It references `Shared.ModuleComposition`, `Shared.Modules`, and `Shared.Naming`.
- `Shared.Messaging` owns integration event, outbox/inbox, subscription, messaging descriptor contracts, and messaging composition feature ids. It references shared primitives plus DI abstractions, not transport adapters.
- `Shared.Tasks` owns task contracts, task descriptor metadata, and task runtime composition feature ids. It does not reference CQRS or runtime adapters.
- `Shared.Caching.Cqrs` owns the optional bridge for flushing deferred cache invalidations after successful CQRS unit-of-work commits.
- `Shared.Tasks.Cqrs` owns the optional bridge for dispatching application commands from task payload handlers.
- `Shared.ProjectionRebuild` owns the task-neutral rebuild loop, checkpoint contracts, source/writer contracts, and metrics; `Shared.ProjectionRebuild.Tasks` adapts that loop to task progress and control messages.
- `Shared.Runtime` owns clock/id abstractions and dependency-free runtime naming helpers.
- `Shared.Tenancy` owns tenant context contracts, tenant options, and tenant errors.
- `Shared.Security` owns dependency-free claim/security constants shared by HTTP adapters and token issuers.
- `Shared.Cqrs` owns command/query contracts, validators, dispatcher contracts, `Unit`, and transactional unit-of-work contracts.
- `Shared.Application.Events` owns domain-event handler and dispatcher contracts. It references `Shared.Domain` only.
- `Shared.Pagination` owns normalized paging request helpers and remains dependency-free.
- `Shared.Application.Composition` owns constrained application assembly registration only. It may reference `Shared.Application.Events`, `Shared.Cqrs`, and small dependency-injection abstractions, but not domain models directly, HTTP, EF, messaging transports, cache backends, logging backends, hosting, or provider packages.
- Adapter projects such as `Shared.Infrastructure`, `Shared.Application.Events.Infrastructure`, `Shared.Cqrs.Infrastructure`, `Shared.Runtime.Infrastructure`, `Shared.Tenancy.Infrastructure`, `Shared.Tenancy.Api.Serilog`, `Shared.Tenancy.Caching`, `Shared.Tenancy.Cqrs`, `Shared.Tenancy.Tasks`, `Shared.Caching.Infrastructure`, `Shared.Caching.Cqrs`, `Shared.Messaging.Infrastructure`, `Shared.Messaging.Nats`, `Shared.Tasks.Infrastructure`, `Shared.ProjectionRebuild.Tasks`, `Shared.Persistence.EntityFrameworkCore`, `Shared.Api.*`, `Shared.Caching.Redis`, `Shared.Messaging.Nats.Aspire`, and `Shared.Logging.Serilog` own concrete runtime packages.

This keeps every module free to depend on shared contracts and primitives without inheriting optional infrastructure choices.

Shared project ownership quick reference:

- `Shared.Infrastructure`: host-level facade that composes the baseline runtime adapters below.
- `Shared.Application.Events.Infrastructure`: domain-event dispatcher implementation.
- `Shared.Cqrs.Infrastructure`: request dispatcher, CQRS pipeline behaviors, command unit-of-work behavior, and CQRS runtime registration.
- `Shared.Runtime.Infrastructure`: default clock and id generator implementations.
- `Shared.Tenancy.Infrastructure`: default/null tenant context, tenant option validation, and baseline tenancy service wiring.
- `Shared.Tenancy.Api.Serilog`: optional tenant-to-HTTP-request-log bridge that contributes tenant id to Serilog diagnostic context without making the base Serilog adapter depend on tenancy.
- `Shared.Tenancy.Caching`: optional tenant-to-cache bridge that resolves tenant-owned cache scope values without making cache infrastructure depend on tenancy.
- `Shared.Tenancy.Cqrs`: optional tenant-to-CQRS logging bridge that contributes tenant context to CQRS log scopes without making CQRS infrastructure depend on tenancy.
- `Shared.Tenancy.Tasks`: optional tenant-to-task execution bridge that prepares tenant context for tenant-scoped task handlers without making task infrastructure depend on tenancy.
- `Shared.Caching.Infrastructure`: HybridCache-backed cache-aside runtime, cache invalidation queue, cache metrics, and cache option validation.
- `Shared.Caching.Cqrs`: optional command pipeline bridge that flushes deferred cache invalidations after successful CQRS unit-of-work commits.
- `Shared.Messaging.Infrastructure`: EF outbox/inbox base helpers, outbox publisher, outbox options, a null event bus, and messaging metrics.
- `Shared.Messaging.Nats`: NATS JetStream publisher/consumer runtime, NATS options, and low-level NATS composition hooks.
- `Shared.Tasks.Infrastructure`: EF task-run store base, task worker/scheduler hosted services, task control loop, task options, and task metrics.
- `Shared.Tasks.Cqrs`: optional task-to-CQRS command dispatcher contract and runtime registration. Hosts compose `AddTaskCqrs()` only when task payload handlers dispatch application commands.
- `Shared.ProjectionRebuild`: task-neutral rebuild runner, source/writer contracts, checkpoint contracts, bounded metrics, and default no-op observer.
- `Shared.ProjectionRebuild.Tasks`: optional task adapter that maps rebuild progress/control polling to `ITaskRuntimeReporter` and `ITaskControlLoop`.
- `Shared.Persistence.EntityFrameworkCore`: EF provider selection, design-time DbContext options, persistence options, and domain-event unit-of-work base.
- `Shared.Observability.Infrastructure`: shared CQRS metric implementations, module-name resolution, and bounded tag normalization. Capability metrics live beside their owning runtime adapters.
- `Shared.Modules`: module descriptor, descriptor builder, descriptor feature base, generic metadata naming/guard helpers, and custom metadata feature support.
- `Shared.ModuleComposition`: profile metadata, composition feature requirements/providers, module requirement validation, and host-level validation extensions.
- `Shared.Authorization`: permission metadata and `WithPermission(...)` / `WithPermissions(...)` / `GetPermissions()` descriptor extensions.
- `Shared.Cqrs`: command/query contracts, validators, dispatcher contracts, `Unit`, and transactional unit-of-work contracts.
- `Shared.Cqrs.Infrastructure`: CQRS dispatcher and pipeline behavior implementations.
- `Shared.Application.Events`: domain-event handler and dispatcher contracts.
- `Shared.Application.Events.Infrastructure`: domain-event dispatcher implementation.
- `Shared.Application.Composition`: constrained application assembly registration.
- `Shared.Naming`: low-level shared naming and identifier syntax primitives.
- `Shared.Numerics`: dependency-free numeric validation helpers shared by domain and contract metadata.
- `Shared.Observability`: vendor-neutral metric, log-property, and tag names.
- `Shared.Pagination`: normalized paging request helpers.
- `Shared.Runtime`: shared runtime abstractions and dependency-free runtime helpers such as clock/id generator contracts and worker-id normalization.
- `Shared.Runtime.Infrastructure`: default runtime implementations for clock and id generator contracts.
- `Shared.Security`: shared claim/security constants.
- `Shared.Caching`: cache-aside contracts, cache key/tag primitives, provider/options contracts, distributed adapter registration marker, and cache descriptor metadata.
- `Shared.Tenancy.Caching`: optional tenant-to-cache scope bridge.
- `Shared.Tenancy.Tasks`: optional tenant-to-task execution context bridge.
- `Shared.Caching.Cqrs`: optional cache-to-CQRS invalidation bridge.
- `Shared.Messaging`: integration event contracts, outbox/inbox contracts, subscription registry contracts, and messaging descriptor metadata.
- `Shared.Tasks`: task payload, handler, control, schedule, run-store, and task descriptor contracts.
- `Shared.Tasks.Cqrs`: optional task-to-CQRS command dispatcher contract and bridge.
- `Shared.Tenancy`: tenant context contracts, tenant options, and tenant errors.
- `Shared.Api`: ASP.NET Core-neutral API primitives and endpoint helpers.
- `Shared.Api.OpenApi`: Swagger/OpenAPI package ownership.
- `Shared.Api.Serilog`: tenant-neutral HTTP request logging enrichment package ownership.
- `Shared.Tenancy.Api.Serilog`: optional tenant-to-request-logging enrichment bridge.
- `Shared.Logging.Serilog`: host logging configuration package ownership.
- `Shared.Caching.Redis`: Redis cache adapter package ownership. It depends only on `Shared.Caching` contracts plus Redis packages, not the HybridCache runtime package.
- `Shared.Messaging.Nats.Aspire`: Aspire/NATS client composition package ownership.
- `Shared.Administration`: backend-agnostic administration contracts and RBAC/audit abstractions.
- `Shared.Administration.Cli`: System.CommandLine administration front-door helpers.
- `Shared.Administration.Api`: administration HTTP front-door helpers.

## Module Projects

Recommended projects:

- `<Module>.Contracts`
- `<Module>.Domain`
- `<Module>.Application`
- `<Module>.Infrastructure`
- `<Module>.Persistence`
- `<Module>.Persistence.SqlServerMigrations`
- `<Module>.Persistence.PostgreSqlMigrations`
- `<Module>.Api`
- `<Module>.Admin.Contracts`
- `<Module>.AdminCli`
- `<Module>.AdminApi`

Not every module needs every project. Keep small modules small.

## Contracts

`<Module>.Contracts` contains DTOs and integration events that other modules or clients may use.

Contract projects use a stable physical folder taxonomy:

- `Api/` for normal public request/response/DTO contracts.
- `Admin/` for admin-facing DTO contracts that must remain backend-free but are used by admin CLI/API flows.
- `Events/` for integration event payloads and subject constants.
- `Metadata/` for module descriptors, permission code strings, contract limits, and other tooling-visible metadata.
- `Types/` for public enum-like or code-list contract types.

The physical folders are for discoverability and architecture tests. File namespaces currently remain `<Module>.Contracts` unless a later deliberate breaking change moves to subnamespaces.

Allowed examples:

- request/response records
- public enum values used by requests
- integration events
- public constants for event names, if needed
- public permission code strings used by module metadata
- module metadata descriptors for tooling and docs

Public contract enums use `Unknown = 0` when they exist. Application handlers must validate incoming enum values instead of treating `Unknown` or an undefined numeric value as a valid domain decision.

Avoid:

- `AdminPermission` typed constants
- domain entities
- repository interfaces
- EF Core models
- command handlers
- endpoint handlers

## Module Metadata

Module metadata is a data contract, not runtime discovery.

Use `ModuleDescriptor` in `.Contracts` when a module has permissions, integration events, inbound subscriptions, cache entries, task metadata, or a persistence schema that should be visible to tests, docs, or tooling.

`Name` is the module identity used for composition, observability, and cross-module metadata. `AdminSurfaceName` is optional and exists for modules whose public administration surface intentionally differs from the module identity, such as the `Administration` module exposing `admin.*` commands and permissions while the module remains named `administration`.

Author descriptors through the builder:

```csharp
public static ModuleDescriptor Descriptor { get; } = ModuleDescriptor
    .Create(Name)
    .WithSchema(Schema)
    .WithPermission(new ModulePermissionDescriptor("catalog.items.read", "Read catalog items.", tenantScoped: true))
    .WithPublishedEvent<CatalogItemCreatedIntegrationEvent>()
    .WithCacheEntries([
        new ModuleCacheDescriptor(ItemsCacheEntry, CacheScope.Tenant, [ItemsCacheTag]),
        new ModuleCacheDescriptor(ItemCacheEntry, CacheScope.Tenant, [ItemsCacheTag]),
    ])
    .Build();
```

Prefer the single-item helpers (`WithPermission`, `WithPublishedEvent`, `WithSubscription`, `WithCacheEntry`, `WithTask`) when metadata naturally belongs near one resource or feature. Use the bulk helpers (`WithPermissions`, `WithPublishedEvents`, `WithSubscriptions`, `WithCacheEntries`, `WithTasks`) when a compact list is clearer. Repeated calls merge within the owning capability feature; duplicate metadata still fails through the capability descriptor.

For metadata that belongs to one local type, prefer the attribute-backed helpers:

- put `IntegrationEventNameAttribute` and `IntegrationEventVersionAttribute` on integration event contract types and use `WithPublishedEvent<TEvent>()`;
- put `IntegrationEventHandlerAttribute` on consumer handler types and register them with `AddIntegrationEventHandler<TEvent,THandler>(consumerModule, producerModule)`;
- put split task attributes such as `TaskNameAttribute`, `TaskPayloadVersionAttribute`, `TaskDescriptionAttribute`, `TaskKindAttribute`, and optional `TaskWorkerGroupAttribute`/`SupportsTaskControlAttribute` on serialized task payload contract types and use `WithTask<TPayload>()` plus `AddTaskHandler<TPayload,THandler>(moduleName)`;
- put `[TenantScoped]` from `Shared.Tenancy` on event or task payload contracts that need tenant context.

These helpers read attributes from known generic types only. They do not scan assemblies, discover modules, register endpoints, start consumers, or compose workers. Keep permissions and cache metadata descriptor-authored until a single local owner type exists for that metadata.

The root descriptor owns only identity and polymorphic capability features. Capability-specific metadata and extensions live beside the capability:

- `Shared.Modules` owns the root descriptor, builder, and custom feature base.
- `Shared.ModuleComposition` owns module profile metadata plus `WithProfile(...)`, `WithProfiles(...)`, `GetCompositionProfiles()`, `SelectModuleProfile(...)`, `ProvideFeature(...)`, `RequireFeature(...)`, `RequireModule(...)`, and `ValidateModuleComposition()`.
- `Shared.Authorization` owns permission metadata plus `WithPermission(...)`, `WithPermissions(...)`, and `GetPermissions()`.
- `Shared.Naming` owns low-level kebab-case segment, module-name, and tenant-id normalization shared by domain events, API/admin composition, CLI command ownership, modules, messaging, caching, and task metadata.
- `Shared.Messaging` owns published-event and subscription metadata plus `IntegrationEventNameAttribute`, `IntegrationEventVersionAttribute`, `IntegrationEventHandlerAttribute`, `WithPublishedEvent(...)`, `WithPublishedEvent<TEvent>()`, `WithPublishedEvents(...)`, `WithSubscription(...)`, `WithSubscription<TEvent>(producerModule, handlerName)`, `WithSubscriptions(...)`, `GetPublishedEvents()`, and `GetSubscriptions()`.
- `Shared.Caching` owns cache metadata plus `WithCacheEntry(...)`, `WithCacheEntries(...)`, and `GetCacheEntries()`.
- `Shared.Caching.Cqrs` owns the optional command pipeline behavior that flushes deferred invalidations after successful CQRS unit-of-work commits.
- `Shared.Tasks` owns task metadata plus `TaskNameAttribute`, `TaskPayloadVersionAttribute`, `TaskDescriptionAttribute`, `TaskKindAttribute`, `TaskWorkerGroupAttribute`, `SupportsTaskControlAttribute`, `WithTask(...)`, `WithTask<TPayload>()`, `WithTasks(...)`, and `GetTasks()`.
- `Shared.Tenancy` owns `[TenantScoped]` and tenancy metadata readers. Base messaging/task packages do not reference tenancy.

This is an intentional extension seam. The root `ModuleDescriptor` is sealed so its identity surface stays stable; new optional shared capabilities should add a `ModuleDescriptorFeature` subtype and builder/read extensions in their own namespace rather than adding another root property or subclassing the root descriptor.
Feature keys are stable and capability-prefixed, for example `authorization.permissions`, `messaging.published-events`, `caching.entries`, and `tasks.handlers`. Custom feature keys should follow the same `<capability>.<entry>` shape to avoid collisions across optional packages.

Rules:

- host composition still calls `AddModule<TModule>()`, `AddAdminModule<TModule>()`, or `AddAdminApiModule<TModule>()` explicitly;
- metadata does not cause services, endpoints, consumers, or admin commands to auto-register;
- cross-module metadata uses strings for subjects and handler names unless the consuming module already has an allowed `.Contracts` reference, and those strings must still pass shared integration-event naming validation;
- descriptors should be kept in sync with module docs and architecture tests.

Descriptor value objects validate public metadata at construction/build time and expose constructor-only properties. Invalid module names, permission codes, event subjects, subscription handler names, cache scopes, task names, duplicate entries, or mismatched published-event subjects should fail as soon as the module contract assembly is loaded.

## Composition Profiles

Profiles describe which shape of a reusable module is being composed. They are metadata plus host validation, not runtime discovery.

Example:

```csharp
builder.AddModule<TenancyModule>();
builder.AddAuthModule(AuthProfile.TenantScoped());
builder.ValidateModuleComposition();
```

`AuthProfile.TenantScoped()` selects the `auth` tenant-scoped profile, provides Auth features, and requires the generic `tenancy.context` feature. `Shared.Tenancy.Infrastructure` provides the baseline context service so CLI/admin hosts can set tenant context explicitly, while `TenancyModule` selects its default profile and additionally provides `tenancy.header-resolution` for HTTP header-based tenant resolution.

For tenant-free projects, compose the global profile explicitly:

```csharp
builder.AddAuthModule(AuthProfile.Global("global"));
builder.ValidateModuleComposition();
```

The global profile omits `TenancyModule`; it does not omit the baseline shared tenant context service. Compose `Shared.Infrastructure` (or `Shared.Tenancy.Infrastructure`) so `ITenantContext` resolves to the configured `Tenancy:LocalDefaultTenantId`, which Auth sets to the global scope value.

Rules:

- profiles live in the owning module's `.Contracts/Metadata` folder when they are part of the public composition surface;
- front-door projects may offer convenience overloads such as `AddAuthModule(AuthProfile profile)`, but hosts still call them explicitly;
- shared adapters may call `ProvideFeature(...)` for generic capabilities they make available;
- profile validation reports selected modules, provided features, required features, and required modules deterministically;
- profile metadata must not register services, map endpoints, start workers, or scan assemblies.

Current reusable/example profiles:

- `CatalogProfiles.Default` provides `catalog.items` and requires tenant context, cache-aside/invalidation services, and outbox infrastructure because its handlers directly depend on those contracts.
- `OrderingProfiles.Default` provides orders plus Ordering-owned catalog item projections. It requires Catalog item facts and tenant context, while NATS consumers and task workers remain optional projection-maintenance enhancements.
- `NotificationsProfiles.Default` provides durable notification history and broadcasts and requires tenant context. Shared live delivery remains separate through `Shared.Notifications.Infrastructure`, `Shared.Notifications.Api`, and `Shared.Notifications.SignalR`.
- `TaskRuntimeProfiles.Default` describes the admin front door and requires the persisted run store, reporter, and control channel provided by `TaskRuntime.Persistence`. Worker-only hosts may compose `TaskRuntime.Persistence` with `Shared.Tasks.Infrastructure` directly and still validate the `tasks.run-store` requirement.

Current adapter feature catalogs live in the capability packages that own the small public contract: `Shared.Caching.CachingCompositionFeatures`, `Shared.Messaging.MessagingCompositionFeatures`, `Shared.Notifications.NotificationsCompositionFeatures`, and `Shared.Tasks.TasksCompositionFeatures`.

Compiled module projects are also listed in `tests/Architecture.Tests/Support/ArchitectureCatalog.cs`. That catalog feeds architecture tests only and must not be used for runtime composition.

## Domain

`<Module>.Domain` contains business rules.

Allowed examples:

- aggregate roots
- entities
- value objects
- domain events
- domain service interfaces
- domain errors

Avoid:

- EF Core
- ASP.NET Core
- NATS
- logging
- configuration

## Application

`<Module>.Application` contains use cases.

Allowed examples:

- commands and queries
- command/query handlers
- validators
- domain event handlers
- application options
- ports required by handlers

Avoid:

- Minimal API route mapping
- EF Core configurations
- provider-specific infrastructure
- NATS implementation types

## Persistence

`<Module>.Persistence` contains EF Core and database-owned behavior.

Allowed examples:

- DbContext
- entity configurations
- repositories
- module unit of work
- outbox store/writer
- provider selection wiring

Persistence projects must not introduce cross-module foreign keys.

## Api

`<Module>.Api` contains module composition and endpoint mapping.

Allowed examples:

- `IModule` implementation
- endpoint groups
- request-to-command mapping
- result-to-HTTP mapping

Endpoints should be thin. Put behavior in commands, handlers, aggregates, and services.

## Admin

`<Module>.AdminCli` contains optional command-line administration front doors.

Allowed examples:

- `IAdminCliModule` implementation
- `System.CommandLine` command mapping
- usage of typed permissions from `<Module>.Admin.Contracts`
- CLI input/output mapping
- request-to-command/query mapping

Avoid:

- business rules
- EF Core configurations
- direct repository access
- references to other module internals

Admin CLI projects are registered explicitly by `Host.AdminCli`, not by `Host.Api`.

## Admin Contracts

`<Module>.Admin.Contracts` contains optional administration contract helpers shared by `.AdminCli` and `.AdminApi`.

Admin contract projects use:

- `Permissions/` for typed `AdminPermission` wrappers.
- `Operations/` for admin operation name constants.

Allowed examples:

- typed `AdminPermission` constants created from public permission code strings
- admin-only operation metadata used by CLI and admin HTTP

Avoid:

- DTOs used by public API clients
- command/query handlers
- EF Core configurations
- command-line or HTTP route mapping

Public `.Contracts` projects must not reference `Shared.Administration`. Keep the generic permission code strings there when metadata needs them, and put `AdminPermission` typed constants in `.Admin.Contracts`.

## AdminApi

`<Module>.AdminApi` contains optional administration HTTP routes.

Allowed examples:

- `IAdminApiModule` implementation
- Minimal API route mapping
- admin HTTP request/response records
- request-to-command/query mapping

Avoid:

- business rules
- EF Core configurations
- direct repository access
- references to other module internals

Admin API projects are registered explicitly by `Host.AdminApi`, not by `Host.Api`.

## Adding a Module

Use:

```powershell
.\eng\new-module.ps1 -Name Billing
```

For persistence:

```powershell
.\eng\new-module.ps1 -Name Billing -Persistence -SqlServerMigrations -PostgreSqlMigrations
```

For a richer optional shell:

```powershell
.\eng\new-module.ps1 -Name Billing -Persistence -SqlServerMigrations -PostgreSqlMigrations -Outbox -Inbox -AdminCli -AdminApi -Cache
```

Then decide explicitly whether to register it in `Host.Api`.
When `-RegisterInHost` is used, the script inserts the public API module at the explicit `// module-scaffold:public-api-modules` composition marker in `src/Host.Api/Program.cs`.
If the module is committed as compiled code, add its projects to `ArchitectureCatalog` so boundary tests cover it.

The scaffold follows current runtime conventions:

- application registration extends `IServiceCollection`; runtime/front-door projects pass host configuration only when application options need it;
- application and persistence registration extensions reject null receivers explicitly;
- application DI uses constrained assembly registration for CQRS handlers, validators, and domain-event handlers; integration-event subscriptions stay explicit because subject names and stable handler names are public contracts;
- persistence DI uses repeat-safe registration so public API, admin API, and CLI surfaces can compose the same module safely;
- persistence registration may extend `IHostApplicationBuilder` because it owns provider/configuration wiring;
- persistence registration calls `AddPersistenceOptions(builder.Configuration)` before provider-specific DbContext setup;
- persisted commands should use `ITransactionalCommand<TResponse>`;
- persistence modules register a module-owned `IUnitOfWork` with the lowercase module name;
- persistence scaffolds use shared design-time EF helpers;
- optional inbox/outbox flags scaffold module-owned tables and stores;
- optional admin flags create explicit admin contracts plus CLI/API composition shells;
- outbox projectors should resolve writers through `IOutboxWriterRegistry`.
