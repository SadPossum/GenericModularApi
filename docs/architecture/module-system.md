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

- `Shared.Domain` and `Shared.ErrorHandling` stay dependency-free.
- `Shared.Application` may reference `Shared.Domain`, `Shared.ErrorHandling`, and small dependency-injection abstractions, but not HTTP, EF, messaging, cache backends, logging backends, hosting, or provider packages.
- Adapter projects such as `Shared.Infrastructure`, `Shared.Api.*`, `Shared.Caching.Redis`, `Shared.Messaging.Nats.Aspire`, and `Shared.Logging.Serilog` own concrete runtime packages.

This keeps every module free to depend on shared contracts and primitives without inheriting optional infrastructure choices.

Shared adapter ownership:

- `Shared.Infrastructure`: generic runtime adapters for EF provider selection, HybridCache, CQRS pipeline wiring, outbox/inbox helpers, NATS publish/consumer runtime abstractions, clocks, IDs, and hosted background loops.
- `Shared.Api`: ASP.NET Core-neutral API primitives and endpoint helpers.
- `Shared.Api.OpenApi`: Swagger/OpenAPI package ownership.
- `Shared.Api.Serilog`: HTTP request logging enrichment package ownership.
- `Shared.Logging.Serilog`: host logging configuration package ownership.
- `Shared.Caching.Redis`: Redis cache adapter package ownership.
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

Use `ModuleDescriptor` in `.Contracts` when a module has permissions, integration events, inbound subscriptions, cache entries, or a persistence schema that should be visible to tests, docs, or tooling.

`Name` is the module identity used for composition, observability, and cross-module metadata. `AdminSurfaceName` is optional and exists for modules whose public administration surface intentionally differs from the module identity, such as the `Administration` module exposing `admin.*` commands and permissions while the module remains named `administration`.

Rules:

- host composition still calls `AddModule<TModule>()`, `AddAdminModule<TModule>()`, or `AddAdminApiModule<TModule>()` explicitly;
- metadata does not cause services, endpoints, consumers, or admin commands to auto-register;
- cross-module metadata uses strings for subjects and handler names unless the consuming module already has an allowed `.Contracts` reference, and those strings must still pass shared integration-event naming validation;
- descriptors should be kept in sync with module docs and architecture tests.

Descriptor value objects validate public metadata at construction time and expose constructor-only properties. Invalid module names, permission codes, event subjects, subscription handler names, cache scopes, duplicate entries, or mismatched published-event subjects should fail as soon as the module contract assembly is loaded.

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
.\eng\new-module.ps1 -Name Billing -Persistence -SqlServerMigrations -PostgreSqlMigrations -Outbox -Inbox -Admin -AdminApi -Cache
```

Then decide explicitly whether to register it in `Host.Api`.
When `-RegisterInHost` is used, the script inserts the public API module at the explicit `// gma:new-module:public-api-modules` composition marker in `src/Host.Api/Program.cs`.
If the module is committed as compiled code, add its projects to `ArchitectureCatalog` so boundary tests cover it.

The scaffold follows current runtime conventions:

- application registration extends `IServiceCollection`; runtime/front-door projects pass host configuration only when application options need it;
- application and persistence registration extensions reject null receivers explicitly;
- application/persistence DI uses repeat-safe registration so public API, admin API, and CLI surfaces can compose the same module safely;
- persistence registration may extend `IHostApplicationBuilder` because it owns provider/configuration wiring;
- persistence registration calls `AddPersistenceOptions(builder.Configuration)` before provider-specific DbContext setup;
- persisted commands should use `ITransactionalCommand<TResponse>`;
- persistence modules register a module-owned `IUnitOfWork` with the lowercase module name;
- persistence scaffolds use shared design-time EF helpers;
- optional inbox/outbox flags scaffold module-owned tables and stores;
- optional admin flags create explicit admin contracts plus CLI/API composition shells;
- outbox projectors should resolve writers through `IOutboxWriterRegistry`.
