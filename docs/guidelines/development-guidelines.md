# Development Guidelines

## Core Rule

Preserve the skeleton goal: modules should stay separate, optional, replaceable, and easy to reason about.

Prefer explicit, boring code over framework cleverness.

## Branching And Versioning

Use `dev` as the normal development branch.

Branch roles:

- `main` is the versioned release baseline. It should receive validated version commits, narrowly scoped docs hotfixes, or emergency repairs.
- `dev` is the integration branch for normal feature work. It can contain focused feature commits, cleanup commits, and staged refactor commits without version markers.
- Short-lived topic branches are optional for risky or parallel work. Create them from `dev`, then merge or squash them back into `dev` when the slice is ready.

Release flow:

1. Finish the slice on `dev`.
2. Run the validation path appropriate to the touched area. Run the full suite and Docker-backed tests before a version commit when persistence, messaging, tasks, Docker infrastructure, or cross-module behavior changed.
3. Switch to `main`, update it from `origin/main`, and squash `dev` into one version commit such as `v0.6.0: add billing module`.
4. Push `main`.
5. Switch back to `dev`, merge `main` into `dev`, and push `dev`.

Merging `main` back into `dev` after each version commit keeps the release commit in `dev` history while preserving the individual development commits that led to it. Avoid resetting or force-pushing shared `dev` unless the branch itself is being repaired intentionally.

If a hotfix lands directly on `main`, merge `main` back into `dev` before continuing feature work.

## Where Things Go

| Thing | Location |
| --- | --- |
| Public request/response DTO | `<Module>.Contracts` |
| Integration event contract | `<Module>.Contracts` |
| Aggregate, entity, value object | `<Module>.Domain` |
| Domain event | `<Module>.Domain.Events` |
| Command/query | `<Module>.Application` |
| Handler | `<Module>.Application.Handlers` |
| Validator | `<Module>.Application.Validation` |
| Adapter interface needed by domain | `<Module>.Domain.Services` |
| Adapter implementation | `<Module>.Infrastructure` |
| DbContext and EF config | `<Module>.Persistence` |
| Minimal API routes | `<Module>.Api` |
| Typed admin permission constants | `<Module>.Admin.Contracts` |
| CLI admin commands | `<Module>.AdminCli` |
| HTTP admin routes | `<Module>.AdminApi` |
| Generic admin contracts | `Shared.Administration` |
| HTTP admin adapter | `Shared.Administration.Api` |
| CLI admin adapter | `Shared.Administration.Cli` |
| Shared module metadata | `Shared.Modules` |
| Shared composition profiles and feature validation | `Shared.ModuleComposition` |
| Shared permission metadata | `Shared.Authorization` |
| Shared naming primitives | `Shared.Naming` |
| Shared numeric primitives | `Shared.Numerics` |
| Shared result/error primitives | `Shared.Results` |
| Shared observability names | `Shared.Observability` |
| Shared clock/id abstractions | `Shared.Runtime` |
| Shared claim/security constants | `Shared.Security` |
| Shared cache contracts | `Shared.Caching` |
| Shared messaging contracts | `Shared.Messaging` |
| Shared user notification contracts | `Shared.Notifications` |
| Shared notification-to-CQRS bridge | `Shared.Notifications.Cqrs` |
| Shared task contracts | `Shared.Tasks` |
| Shared task-to-CQRS bridge | `Shared.Tasks.Cqrs` |
| Shared projection rebuild contracts/runtime | `Shared.ProjectionRebuild` |
| Shared projection rebuild task bridge | `Shared.ProjectionRebuild.Tasks` |
| Shared CQRS contracts | `Shared.Cqrs` |
| Shared tenancy contracts | `Shared.Tenancy` |
| Shared application assembly registration | `Shared.Application.Composition` |
| Shared domain-event application contracts | `Shared.Application.Events` |
| Shared domain-event dispatcher runtime | `Shared.Application.Events.Infrastructure` |
| Shared paging request helpers | `Shared.Pagination` |
| Shared domain abstractions | `Shared.Domain` |
| Shared host-level runtime facade | `Shared.Infrastructure` |
| Shared CQRS runtime | `Shared.Cqrs.Infrastructure` |
| Shared clock/id runtime | `Shared.Runtime.Infrastructure` |
| Shared cache runtime | `Shared.Caching.Infrastructure` |
| Shared cache-to-CQRS bridge | `Shared.Caching.Cqrs` |
| Shared tenancy-to-cache scope bridge | `Shared.Tenancy.Caching` |
| Shared tenancy-to-CQRS logging bridge | `Shared.Tenancy.Cqrs` |
| Shared tenancy-to-task execution bridge | `Shared.Tenancy.Tasks` |
| Shared messaging runtime | `Shared.Messaging.Infrastructure` |
| Shared NATS messaging transport | `Shared.Messaging.Nats` |
| Shared notification runtime | `Shared.Notifications.Infrastructure` |
| Shared notification SSE adapter | `Shared.Notifications.Api` |
| Shared notification SignalR adapter | `Shared.Notifications.SignalR` |
| Shared task runtime | `Shared.Tasks.Infrastructure` |
| Shared EF persistence helpers | `Shared.Persistence.EntityFrameworkCore` |
| Shared default tenancy runtime | `Shared.Tenancy.Infrastructure` |
| Shared CQRS metric implementations and bounded tag helpers | `Shared.Observability.Infrastructure` |
| HTTP helpers | `Shared.Api` |
| OpenAPI/Swagger helpers | `Shared.Api.OpenApi` |
| HTTP request logging enrichment | `Shared.Api.Serilog` |
| Tenant HTTP request logging enrichment | `Shared.Tenancy.Api.Serilog` |
| Host Serilog configuration | `Shared.Logging.Serilog` |
| Aspire NATS client composition | `Shared.Messaging.Nats.Aspire` |
| Tests for boundaries | `Architecture.Tests` |
| Module metrics class | Owning module Application or Infrastructure project |
| Exporter configuration | `ServiceDefaults` |

## Adding a Feature to a Module

1. Add or update contract DTOs only if the public API changes.
2. Add domain behavior to aggregate/entity/value object.
3. Add command/query and handler.
4. Add validator.
5. Add persistence changes and migrations if needed.
6. Map or update endpoint.
7. Add unit tests.
8. Add integration tests if persistence, tenancy, auth, or messaging behavior changes.
9. Update module docs.

## Adding a Module

Use the scaffolder:

```powershell
.\eng\new-module.ps1 -Name Billing
```

Then manually decide:

- whether the module needs persistence;
- whether it needs integration events;
- whether it consumes integration events and needs an inbox table;
- whether it needs admin CLI/API front doors;
- whether it needs explicit cache-aside reads;
- whether it is tenant-scoped;
- whether `Host.Api` should register it by default.

Optional modules must be explicit host decisions.

`eng/new-module.ps1` scaffolds the current project shape, module metadata, and optional persistence/admin/cache/inbox/outbox shells. It still does not invent domain behavior, aggregate models, commands, queries, or host registration decisions. Use the compiled Catalog and Ordering examples as the richer reference for stored entities, admin surfaces, cache keys/tags, provider migrations, integration events, inbound subscriptions, and cross-module projections.

When a new module becomes compiled code, update `Architecture.Tests/Support/ArchitectureCatalog.cs` in the same change. The catalog is test/tooling metadata only; it must not become runtime module discovery.

## Dependency Rules

Do:

- depend on `Shared.*` abstractions;
- depend on your own module projects;
- depend on another module's `.Contracts` project when truly needed.

Do not:

- reference another module's Domain, Application, Infrastructure, Persistence, or Api project;
- expose EF entities through contracts;
- make application projects depend on `IHostApplicationBuilder` or `Microsoft.Extensions.Hosting`; expose `IServiceCollection` registration instead;
- publish directly to NATS from domain or application code;
- reference SignalR, SSE, or notification front-door adapter packages from modules;
- reference NATS, Redis, Prometheus, OpenTelemetry exporters, Serilog sinks, or cache-backend packages from module projects;
- call `Guid.NewGuid()`, `DateTimeOffset.UtcNow`, or `DateTime.UtcNow` from feature-module code; use `IIdGenerator` and `ISystemClock` instead;
- let endpoints contain business rules.

## Result and Errors

Use `Result` and `Result<T>` for expected failures.

Guidelines:

- error codes are stable public contracts; use dotted identifiers such as `Auth.MemberNotFound` or `Validation.Failed`;
- error codes are case-preserving, must contain at least one `.`, and may contain only ASCII letters, digits, and `.`;
- error messages are short client/operator text; keep detailed diagnostics in logs, not in `Error.Message`;
- domain invariants return domain errors;
- application validation returns application errors;
- not-found reads return explicit errors and HTTP/admin adapters map those errors to `404` where appropriate;
- do not model CQRS request payloads as nullable `Result<T?>` successes; `Result<T>` rejects successful null values at construction;
- do not pass `null` or `Error.None` to failure factories; failed results must carry a real `Error`;
- unexpected infrastructure failures may throw;
- endpoints should convert results to HTTP through shared helpers;
- public API modules should use `ApiErrorStatusCodeMap` at the front-door edge when an error needs `401`, `403`, `404`, `409`, or another non-default status. Do not add HTTP status concepts to domain or application errors.
- create status map entries with `ApiErrorStatusCode`; entries validate dotted error codes and enforce `4xx`/`5xx` HTTP status codes at construction, while maps remain the duplicate/default-entry guard.

## Request Boundaries

Normalize request-bound paging inputs with `Shared.Pagination.PageRequest` before calling repositories or cache-key builders. Repository read ports should accept `PageRequest` rather than raw `page` / `pageSize` integers, and EF queries should use `PageRequest.SkipCount` plus `PageRequest.PageSize`.

Do not copy local `Math.Max`/`Math.Clamp` paging rules into handlers. Shared defaults and maximums keep API, admin API, CLI, cache keys, and EF `Skip` arithmetic aligned. `PageRequest` normalizes through `Normalize(...)`, its public constructor, and its default struct instance.

Use `ICommandValidator<TCommand>` and `IQueryValidator<TQuery>` for request-shape checks that should fail before handlers touch repositories, caches, or aggregates. Keep deeper business invariants in aggregates and domain services.

Do not add FluentValidation or another parallel validation framework to module API front doors by default. ADR 0007 keeps the skeleton's validation path on the shared CQRS validator contract so API, Admin API, CLI, tests, and generated modules stay aligned.

Do not map unknown enum values to a valid domain value by default. Application handlers should return explicit application errors for unsupported enum values, and CLI adapters should parse textual options inside the authorized operation path so denied actors do not receive validation details.

If a module needs forward-compatible enum handling, make it deliberate: use `Unknown = 0`, a smart enum, or a small value-object/code-list type with tests and docs. The important rule is that unknown input must never become a meaningful domain value by accident.

If the project adopts custom enums, prefer one small shared pattern over per-module one-offs: define parsing, display name, persistence value, unknown/compatibility behavior, and architecture tests before any module relies on it.

Public module contract enums should own their JSON converter and wire-name helper in the same `.Contracts` package. Do not rely on host-wide JSON enum options, because optional modules should keep stable API/event text values when composed in different hosts.

Provider/configuration enums should also reserve `Unknown = 0` when a bad or missing value could otherwise select a real backend. Option validators must reject `Unknown` and undefined numeric values unless the option is intentionally ignored because the whole concern is disabled. Persisted enum renumbering requires provider-specific compatibility migrations and data backfills in the same change.

When domain text fields have persistence limits, expose a named domain/application constant and validate before persistence. EF mappings should reference the same constant instead of duplicating raw lengths.

When decimal values are persisted with explicit precision and scale, expose named constants and validate that domain values fit without rounding before persistence. Database rounding or provider overflow should not be the first signal for invalid business values.

Guid-backed identity value objects should reject `Guid.Empty` in their public constructor. If the identity is a struct, remember `default(TId)` still exists; aggregates and entities should keep defensive empty-id checks for default values and persistence materialization.

## Configuration

Add options classes for structured config.

Rules:

- options section names should match module or concern names;
- no magic strings scattered through handlers;
- secrets must come from environment, user secrets, or deployment secret stores;
- do not commit real secrets.

## Tenancy

Before writing tenant-scoped code, answer:

- Is this endpoint tenant-scoped?
- Does the aggregate store `TenantId`?
- Are unique indexes tenant-local?
- Do queries preserve tenant filters?
- Are tenant-owned cache keys paired with `CachingCompositionFeatures.TenantScopeRequired(...)`?
- Are integration events tenant-scoped?

Use `TenantIds` in domain, application, infrastructure, and front-door code when accepting or storing a tenant id from aggregates, headers, commands, events, or configuration. Tenant ids are trimmed, case-preserving, capped at 128 characters, and reject whitespace or control characters to match persistence mappings.

Tenant-owned models should make ownership visible with `TenantAggregateRoot<TId>`, `TenantEntity<TId>`, or a direct `ITenantScoped` implementation. Do not hide tenancy behind shadow EF properties or host-side reflection. Tenant-aware EF modules should inherit `TenantAwareDbContext<TContext>` and call `ApplyTenantConventions(modelBuilder)` so `TenantId` mapping, the named `TenantFilter`, and write-side tenant guards stay centralized.

Infrastructure records that contain tenant ids are not automatically tenant-owned. Outbox, inbox, task runtime, audit, and projection-control tables should be classified deliberately before applying tenant filters.

If yes, tests must cover tenant isolation.

## Persistence

Rules:

- each module owns its schema;
- each module owns provider-specific migrations;
- keep `Microsoft.EntityFrameworkCore.Design` and `IDesignTimeDbContextFactory<TContext>` in provider-specific migration projects, not runtime persistence projects;
- add migrations with `eng/add-migration.ps1 -Module <Module> -Provider SqlServer|PostgreSql -Name <Name>`;
- run `eng/check-migrations.ps1 -NoBuild` after EF mapping changes so SQL Server and PostgreSQL snapshots stay aligned with the model;
- use `TenantAwareDbContext<TContext>` plus `ApplyTenantConventions(modelBuilder)` for tenant-scoped EF entities;
- index tenant-scoped read paths with the tenant id first when queries filter by tenant and sort or join by another column;
- no cross-module foreign keys;
- do not use `EnsureCreated` in integration tests;
- do not auto-apply migrations at API startup.

## Messaging

Rules:

- domain raises domain events;
- domain events inherit `DomainEvent` or `TenantDomainEvent` so common event id, occurrence time, and tenant id rules are not repeated per module;
- application handlers map domain events to integration events;
- application handlers resolve the owning writer through `IOutboxWriterRegistry`;
- module outbox writer stores integration events;
- public integration events inherit `IntegrationEvent` so event id, occurrence time, event name, and version validation stay centralized;
- tenant-owned integration events inherit `TenantIntegrationEvent` from `Shared.Tenancy.Messaging`; compose `AddTenantAwareMessaging()` in hosts that publish or consume them;
- hosted publisher sends to `IEventBus` only in hosts that explicitly opt into publishing;
- consumers implement `IIntegrationEventHandler<TEvent>`;
- each consuming module owns an inbox table and registers an `IInboxStore`;
- consumer handlers update local module state or projections idempotently;
- EF-backed modules map outbox rows through `ConfigureOutboxMessage(...)` and inbox rows through `ConfigureInboxMessage(...)` instead of repeating message keys, indexes, and length limits;
- shared envelope/outbox/inbox record constructors validate event ids, subject shape, event versions, generic scope ids, and handler/event names; do not bypass them with ad hoc anonymous transport payloads;
- custom inbox stores return `InboxProcessResult` through its factories and never hand-build processing outcomes;
- NATS stays behind infrastructure.

Do not inject a bare `IOutboxWriter` into application code. Multiple modules can be composed in one host, so writer selection must be module-qualified.

## Notifications

Rules:

- use notifications only for front-door user delivery, not module-to-module command/query communication;
- keep durable business facts in domain events, integration events, outbox, NATS, and inbox;
- enqueue notification intent through `IUserNotificationRequestQueue` from transactional command handlers only for best-effort live delivery;
- publish `UserNotificationRequestedIntegrationEvent` from `Notifications.Contracts` through the producing module outbox when notification history creation must be durable;
- publish through `IUserNotificationPublisher` only from front doors, workers, or other code that is already safely outside the current database commit;
- put reusable notification payload contracts in the owning module `.Contracts` project and make them implement `IUserNotificationPayload`;
- put notification identity on the payload type with `NotificationNameAttribute`, `NotificationVersionAttribute`, and `NotificationDescriptionAttribute`;
- keep notification names normalized to lowercase dotted segments such as `catalog.item-updated`;
- do not include passwords, access tokens, refresh tokens, token hashes, or raw secrets in payloads, titles, or bodies;
- do not rely on notifications for authorization, tenant resolution, or guaranteed delivery;
- compose the optional `Notifications` module when users need persisted history or read/unread state;
- treat shared-publisher notification history as durable only after a publish request reaches the shared publisher; use outbox/NATS/inbox and the Notifications request event for guaranteed creation from business facts;
- do not reference `Shared.Notifications.Api`, `Shared.Notifications.SignalR`, SignalR packages, or ASP.NET streaming internals from modules;
- compose `AddUserNotificationsCqrs()` in hosts whose command handlers enqueue notification requests;
- compose `AddUserNotificationServerSentEvents()` and `AddUserNotificationSignalR()` only in hosts that need live user delivery.

The CQRS bridge flushes queued notification requests only after a successful command result and unit-of-work commit. It is scoped, in-process, and best-effort; it prevents before-commit live sends, but it is not the authoritative durable business fact. The optional `Notifications` module stores history for publish requests that reach the publisher and can also consume durable `UserNotificationRequestedIntegrationEvent` messages through its inbox.

## Tasks And Daemons

Rules:

- declare owned task and daemon metadata with split task attributes on the serialized payload contract, then reference it through `ModuleDescriptor.Create(...).WithTask<TPayload>().Build()`;
- keep task payloads that are module metadata or externally enqueueable in the owning module `.Contracts` project;
- register task handlers explicitly through the attribute-backed `AddTaskHandler<TPayload,THandler>(moduleName)` overload from the owning module application registration;
- keep payload code independent from scheduler packages, HTTP, CLI, and other module internals;
- use `TaskExecutionContext` for run identity, tenant, node, worker id, worker group, attempt, correlation, and cancellation intent;
- mark tenant-scoped task payloads with `TenantScopedAttribute` from `Shared.Tenancy` and compose `AddTenantTaskExecutionContext()` from `Shared.Tenancy.Tasks` only in worker hosts that actually run them;
- use explicit task payload versions when changing payload shape; keep old handlers registered until old queued work is drained;
- use deduplication keys for operator/API/schedule paths where duplicate active work would be harmful;
- let code-defined schedules use the default version-aware dedupe key shape unless the module has a documented reason to override it: `schedule:<module>:<task>:<schedule>:v<payload-version>:<occurrence>`;
- expose or accept task run statuses through `TaskRunStatusNames` wire names such as `retry-scheduled`; keep enum names as compatibility input only;
- report heartbeat and progress through `ITaskRuntimeReporter`;
- read system-to-runner control messages through `ITaskControlLoop` or `TaskControlLoopExtensions`;
- dispatch normal application commands from payload code through `ITaskCommandDispatcher` from `Shared.Tasks.Cqrs` or CQRS contracts, and compose `AddTaskCqrs()` only in hosts whose task handlers need it;
- adapt projection rebuilds to task progress/control through `TaskProjectionRebuildRunner<TSnapshot>` from `Shared.ProjectionRebuild.Tasks`;
- keep runtime stores behind `ITaskRunStore` and use `TaskRunStatusTransitions` for claim, retry, cancellation, and terminal-state rules;
- persist requester metadata from `TaskRunRequest.RequestedBy` when the runtime owns a durable store;
- compose `AddTaskRuntimePersistence()` and `AddTaskWorkerRuntime()` only in hosts that should run tasks;
- compose `AddTaskRunScheduling()` only in hosts that should enqueue code-defined schedules;
- add external scheduler adapters only as explicit optional infrastructure projects.

## Administration

Rules:

- keep admin features optional and composed by `Host.AdminCli`;
- keep admin HTTP APIs optional and composed by `Host.AdminApi`;
- keep `Host.Api` free of admin module registration unless a future plan explicitly adds admin APIs;
- keep public permission code strings and permission metadata in `.Contracts` when other tooling needs them;
- declare typed `AdminPermission` constants in `.Admin.Contracts` so `.Contracts` does not reference `Shared.Administration`;
- declare admin operation names as dotted lowercase constants and create operations through `AdminOperation.Create`;
- normalize RBAC actor ids, tenant ids, permission codes, and operation names through `AdminActor`, `TenantIds`, `AdminPermission`, and `AdminOperation`, including in persistence entities. `AdminActor` is factory-created; do not add public constructors or local actor-id trimming;
- create admin audit records through `AdminAuditRecord` and use `AdminAuditResults` constants for result values;
- create admin authorization decisions through `AdminAuthorizationResult.Allowed()` or `.Denied(reason)`;
- keep admin EF max lengths wired to shared value constants such as `AdminActor.MaxLength`, `AdminPermission.MaxLength`, `AdminOperation.MaxLength`, and `TenantIds.MaxLength`;
- keep admin value-object length checks explicit instead of hiding persistence limits inside regex quantifiers;
- route admin commands through the same application commands/queries as other module use cases;
- route admin HTTP endpoints through the same application commands/queries as CLI;
- keep business rules in aggregates/application handlers, not command-line mapping code;
- route admin CLI messages, errors, generated-password output, and prompts through `AdminCliOutput`; module admin front doors should not write directly to `Console` except for terminal input reads such as hidden password capture;
- require `--tenant` for tenant-scoped admin operations;
- use `AdminApiExecutor` for admin HTTP tenant enforcement, and do not add `.RequireTenant()` to admin API routes;
- keep auditable admin HTTP validation inside the executor action so unauthorized callers receive authorization-shaped responses and failed attempts are audited;
- keep admin API tenant claim binding in `AdminApiExecutor`: if the configured tenant claim is present it must match the requested tenant, and if it is absent RBAC still decides access;
- pass an explicit `ApiErrorStatusCodeMap` to `AdminApiExecutor` when an admin operation has expected `404` or `409` outcomes. Authorization, tenant, audit, and unexpected-failure status mapping stays centralized in the executor;
- keep first-owner bootstrap in admin CLI unless a separate ADR explicitly approves an HTTP bootstrap flow;
- keep generated password responses disabled for admin HTTP unless explicitly configured for a controlled environment;
- require `--yes` for destructive non-interactive commands;
- never log or audit passwords, tokens, token hashes, refresh tokens, or raw secrets;
- keep `System.CommandLine` references isolated to `.AdminCli`, `Shared.Administration.Cli`, and `Host.AdminCli`;
- keep admin HTTP route mapping isolated to `.AdminApi`, `Shared.Administration.Api`, and `Host.AdminApi`.

## Observability

Rules:

- use `ILogger<T>` for logging;
- use `IMeterFactory` for metrics;
- use meter and activity-source names under `{ApplicationIdentity:Namespace}.*`; `gma.*` is only the skeleton default;
- attach module metadata with `.WithModuleName(this.Name)` to route groups; endpoint metadata validates the same lowercase module-name shape as messaging and module descriptors;
- keep metric tags bounded;
- do not add tenant, user, token, message, or request ids as metric tags;
- keep Prometheus, Loki, Grafana, and OpenTelemetry exporter dependencies outside modules.
- keep notification metrics bounded to module/operation/provider/result style tags; never tag tenant id, user id, notification id, or payload fields.
- fail-open infrastructure paths should record metrics first and must not become fail-closed because a logging/export provider throws;
- admin operation infrastructure must return shaped operation results for expected authorization/validation/action/audit outcomes even when logging fails.
- CQRS logging must preserve the command/query result or original command/query exception even when logging scopes or log writes fail.
- outbox publishing must update message state before or independently from observability, so logging failures cannot skip retry bookkeeping.
- event bus adapters must never throw after a broker confirms publish success because of success logging or diagnostics.
- consumer loops must keep ack/nak/terminate and retry decisions independent from diagnostic logging.

## Code Style

- Keep nullable enabled.
- Keep warnings as errors.
- Prefer small, explicit classes.
- Keep application-layer DI registration host-agnostic. Use `IServiceCollection`; pass `IConfiguration` only for application-owned options.
- Use `AddApplicationServicesFromAssembly(typeof(DependencyInjection).Assembly)` from module application registration for CQRS handlers, validators, and domain-event handlers. This is the project's only default reflection-based application registration convention.
- Keep integration-event subscriptions explicit with `AddIntegrationEventHandler<TEvent,THandler>(consumerModule, producerModule)`; put event identity/version on the event contract through `EventType`/`EventVersion` constants plus `IntegrationEventNameAttribute` and `IntegrationEventVersionAttribute`, durable handler identity on the handler through `IntegrationEventHandlerAttribute`, and tenant behavior through `[TenantScoped]` from `Shared.Tenancy` when needed.
- Keep user-notification metadata local to the notification payload through `NotificationNameAttribute`, `NotificationVersionAttribute`, and `NotificationDescriptionAttribute`; module descriptors should call `WithUserNotification<TPayload>()` rather than repeat names and versions.
- Keep one application handler class per file under `<Module>.Application/Handlers`.
- Keep one public contract type per file under `<Module>.Contracts`.
- Add comments only when they explain non-obvious decisions.
- Do not add abstractions unless they remove real complexity or preserve replaceability.
- Keep real time and random IDs behind shared infrastructure abstractions. Feature modules should depend on `ISystemClock` and `IIdGenerator` for deterministic behavior and testability.
