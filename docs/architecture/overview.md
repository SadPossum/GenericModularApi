# Architecture Overview

GenericModularApi is a modular monolith skeleton. It keeps deployment simple while preserving module boundaries that can survive growth.

## Goals

- Keep domains and features optional.
- Make modules easy to add, remove, and replace.
- Avoid assembly scanning and hidden registration.
- Keep domain/application code independent of web, EF Core, and NATS details.
- Use reliable outbox publishing for cross-boundary integration events.
- Support shared-database tenancy from the start.
- Keep metrics, logging, and tracing vendor-neutral inside modules.
- Keep caching explicit, optional, tenant-safe, and provider-independent inside modules.

## Current Shape

```text
src/
  Host.Api/
  Host.AdminCli/
  Host.AdminApi/
  AppHost/
  ServiceDefaults/
  Shared/
    Shared.Administration/
    Shared.Administration.Api/
    Shared.Administration.Cli/
    Shared.Api/
    Shared.Api.OpenApi/
    Shared.Api.Serilog/
    Shared.Application.Composition/
    Shared.Application.Events/
    Shared.Application.Events.Infrastructure/
    Shared.Authorization/
    Shared.Caching/
    Shared.Caching.Infrastructure/
    Shared.Caching.Redis/
    Shared.Cqrs/
    Shared.Cqrs.Infrastructure/
    Shared.Domain/
    Shared.Results/
    Shared.Infrastructure/
    Shared.Logging.Serilog/
    Shared.Messaging/
    Shared.Messaging.Infrastructure/
    Shared.Messaging.Nats/
    Shared.Messaging.Nats.Aspire/
    Shared.Modules/
    Shared.Naming/
    Shared.Numerics/
    Shared.Observability/
    Shared.Observability.Infrastructure/
    Shared.Pagination/
    Shared.Persistence.EntityFrameworkCore/
    Shared.ProjectionRebuild/
    Shared.ProjectionRebuild.EntityFrameworkCore/
    Shared.Runtime/
    Shared.Runtime.Infrastructure/
    Shared.Security/
    Shared.Tasks/
    Shared.Tasks.Cqrs/
    Shared.Tasks.Infrastructure/
    Shared.Tenancy/
    Shared.Tenancy.Infrastructure/
  Modules/
    Auth/
      Auth.Contracts/
        Api/
        Admin/
        Events/
        Metadata/
        Types/
      Auth.Domain/
      Auth.Application/
      Auth.Infrastructure/
      Auth.Persistence/
      Auth.Persistence.SqlServerMigrations/
      Auth.Persistence.PostgreSqlMigrations/
      Auth.Api/
      Auth.Admin.Contracts/
        Operations/
        Permissions/
      Auth.AdminCli/
      Auth.AdminApi/
    Administration/
      Administration.Application/
      Administration.Persistence/
      Administration.Persistence.SqlServerMigrations/
      Administration.Persistence.PostgreSqlMigrations/
      Administration.AdminCli/
      Administration.AdminApi/
    Catalog/
      Catalog.Contracts/
      Catalog.Domain/
      Catalog.Application/
      Catalog.Persistence/
      Catalog.Persistence.SqlServerMigrations/
      Catalog.Persistence.PostgreSqlMigrations/
      Catalog.Api/
      Catalog.Admin.Contracts/
      Catalog.AdminCli/
      Catalog.AdminApi/
    Ordering/
      Ordering.Contracts/
      Ordering.Domain/
      Ordering.Application/
      Ordering.Persistence/
      Ordering.Persistence.SqlServerMigrations/
      Ordering.Persistence.PostgreSqlMigrations/
    TaskRuntime/
      TaskRuntime.Contracts/
      TaskRuntime.Application/
      TaskRuntime.Persistence/
      TaskRuntime.Persistence.SqlServerMigrations/
      TaskRuntime.Persistence.PostgreSqlMigrations/
      TaskRuntime.Admin.Contracts/
      TaskRuntime.AdminCli/
      TaskRuntime.AdminApi/
    TaskSamples/
      TaskSamples.Contracts/
      TaskSamples.Application/
    Tenancy/
      Tenancy.Contracts/
      Tenancy.Api/
tests/
  Shared.Tests/
  Auth.Tests/
  Architecture.Tests/
  Integration.Tests/
```

## Runtime Composition

`Host.Api` is the composition root:

1. Adds optional infrastructure adapters such as Redis, OpenAPI, caching runtime, messaging runtime, and configured NATS publishing.
2. Adds shared core infrastructure.
3. Optionally starts NATS/JetStream publishing through the messaging adapter.
4. Explicitly registers optional modules.
5. Maps module endpoints.

Current host registration:

```csharp
builder.AddRedisCaching(); // no-op unless Redis caching is enabled
builder.AddCachingInfrastructure();
builder.AddSharedInfrastructure();
builder.AddMessagingInfrastructure();
builder.AddConfiguredNatsJetStreamMessaging(); // no-op unless NATS publishing is enabled
builder.Services.AddApiSecurityDefaults(); // no default scheme; Auth or another adapter supplies one
builder.AddModule<TenancyModule>();
builder.AddModule<AuthModule>();
builder.AddSharedOpenApi();
app.UseSharedOpenApi(); // serves Swagger only in Development
app.MapModules();
```

`Host.AdminCli` is a separate optional composition root for administrative commands:

```csharp
builder.AddSharedAdministrationCli();
builder.AddRedisCaching(); // no-op unless Redis caching is enabled
builder.AddCachingInfrastructure();
builder.AddSharedInfrastructure();
builder.AddMessagingInfrastructure(); // outbox writer registry without hosted publishers
builder.AddAdminModule<AdministrationAdminCliModule>();
builder.AddAdminModule<AuthAdminCliModule>();
```

It does not map public API endpoints.

`Host.AdminApi` is a separate optional composition root for administrative HTTP APIs:

```csharp
builder.Services.AddSharedAdministrationApi(builder.Configuration);
builder.AddRedisCaching(); // no-op unless Redis caching is enabled
builder.AddCachingInfrastructure();
builder.AddSharedInfrastructure();
builder.AddMessagingInfrastructure();
builder.AddAdminApiModule<AdministrationAdminApiModule>();
builder.AddAdminApiModule<AuthAdminApiModule>();
app.MapAdminApiModules();
```

`Host.Api` still does not map admin routes.

Runtime project dependency ownership:

- `Host.Api` and `Host.AdminApi` should have no direct package references; they compose module front doors and shared adapters.
- `Host.AdminCli` owns only CLI-hosting packages and composes admin CLI modules plus shared runtime adapters.
- `ServiceDefaults` owns local observability, service-discovery, HTTP resilience, and Prometheus scrape endpoint packages.
- `AppHost` owns Aspire hosting packages and references runnable hosts, not module internals.

## Dependency Direction

Allowed direction:

```text
Host.Api
  -> Modules.*.Api
  -> Modules.*.Application
  -> Modules.*.Domain

Modules.*.Persistence
  -> Modules.*.Application
  -> Modules.*.Domain

Shared.Administration
  -> Shared.Naming
  -> Shared.Results
  -> Shared.Runtime
  -> Shared.Tenancy

Shared.Administration.Api
  -> Shared.Administration
  -> Shared.Api
  -> Shared.Cqrs
  -> Shared.Results
  -> Shared.Naming
  -> Shared.Security
  -> Shared.Tenancy

Shared.Administration.Cli
  -> Shared.Administration
  -> Shared.Cqrs
  -> Shared.Results
  -> Shared.Naming
  -> Shared.Runtime

Shared.Api
  -> Shared.Results
  -> Shared.Naming
  -> Shared.Tenancy

Shared.Api.OpenApi
  -> no project references

Shared.Api.Serilog
  -> Shared.Api
  -> Shared.Observability
  -> Shared.Tenancy

Shared.Application.Composition
  -> Shared.Application.Events
  -> Shared.Cqrs

Shared.Application.Events
  -> Shared.Domain

Shared.Application.Events.Infrastructure
  -> Shared.Application.Events
  -> Shared.Domain

Shared.Authorization
  -> Shared.Modules
  -> Shared.Naming

Shared.Caching
  -> Shared.Modules
  -> Shared.Naming

Shared.Caching.Infrastructure
  -> Shared.Caching
  -> Shared.Cqrs
  -> Shared.Cqrs.Infrastructure
  -> Shared.Naming
  -> Shared.Observability
  -> Shared.Observability.Infrastructure
  -> Shared.Results
  -> Shared.Runtime
  -> Shared.Runtime.Infrastructure
  -> Shared.Tenancy

Shared.Caching.Redis
  -> Shared.Caching.Infrastructure

Shared.Cqrs
  -> Shared.Results

Shared.Cqrs.Infrastructure
  -> Shared.Cqrs
  -> Shared.Results
  -> Shared.Naming
  -> Shared.Observability
  -> Shared.Observability.Infrastructure
  -> Shared.Runtime.Infrastructure
  -> Shared.Tenancy
  -> Shared.Tenancy.Infrastructure

Shared.Domain
  -> Shared.Naming
  -> Shared.Numerics

Shared.Infrastructure
  -> Shared.Application.Events.Infrastructure
  -> Shared.Cqrs.Infrastructure
  -> Shared.Runtime.Infrastructure
  -> Shared.Tenancy.Infrastructure

Shared.Logging.Serilog
  -> no project references

Shared.Messaging
  -> Shared.Modules
  -> Shared.Naming
  -> Shared.Numerics

Shared.Messaging.Infrastructure
  -> Shared.Messaging
  -> Shared.Naming
  -> Shared.Observability
  -> Shared.Observability.Infrastructure
  -> Shared.Runtime
  -> Shared.Runtime.Infrastructure

Shared.Messaging.Nats
  -> Shared.Messaging
  -> Shared.Messaging.Infrastructure
  -> Shared.Naming
  -> Shared.Runtime
  -> Shared.Tenancy

Shared.Messaging.Nats.Aspire
  -> Shared.Messaging.Nats

Shared.Modules
  -> Shared.Naming

Shared.Naming
  -> no project references

Shared.Numerics
  -> no project references

Shared.Observability
  -> Shared.Naming

Shared.Observability.Infrastructure
  -> Shared.Naming
  -> Shared.Observability
  -> Shared.Runtime

Shared.Pagination
  -> no project references

Shared.Persistence.EntityFrameworkCore
  -> Shared.Application.Events
  -> Shared.Cqrs
  -> Shared.Domain
  -> Shared.Naming
  -> Shared.Tenancy

Shared.ProjectionRebuild
  -> Shared.Naming
  -> Shared.Observability
  -> Shared.Observability.Infrastructure
  -> Shared.Runtime
  -> Shared.Tasks

Shared.ProjectionRebuild.EntityFrameworkCore
  -> Shared.Naming
  -> Shared.ProjectionRebuild

Shared.Results
  -> no project references

Shared.Runtime
  -> Shared.Naming

Shared.Runtime.Infrastructure
  -> Shared.Naming
  -> Shared.Runtime

Shared.Security
  -> no project references

Shared.Tasks
  -> Shared.Modules
  -> Shared.Naming

Shared.Tasks.Cqrs
  -> Shared.Cqrs
  -> Shared.Results
  -> Shared.Tasks

Shared.Tasks.Infrastructure
  -> Shared.Cqrs
  -> Shared.Cqrs.Infrastructure
  -> Shared.Results
  -> Shared.Observability
  -> Shared.Observability.Infrastructure
  -> Shared.Runtime
  -> Shared.Runtime.Infrastructure
  -> Shared.Tasks.Cqrs
  -> Shared.Tasks
  -> Shared.Tenancy

Shared.Tenancy
  -> Shared.Modules
  -> Shared.Results

Shared.Tenancy.Infrastructure
  -> Shared.Naming
  -> Shared.Tenancy
```

Cross-module dependencies must go through:

- `<OtherModule>.Contracts`
- integration events
- shared abstractions

Do not reference another module's Domain, Application, Persistence, Infrastructure, or Api project.

## Request Flow

```text
HTTP endpoint
  -> command or query
  -> IRequestDispatcher
  -> pipeline behaviors
  -> handler
  -> aggregate/repository
  -> unit of work
  -> domain event dispatcher
  -> outbox writer
  -> EF Core SaveChanges
  -> hosted outbox publisher
  -> IEventBus
  -> NATS JetStream
```

Observability follows a separate adapter flow:

```text
ILogger / IMeterFactory / ActivitySource
  -> ServiceDefaults
  -> optional Prometheus scrape and OTLP export
  -> deployment-selected backends such as Loki
```

Caching follows the same contract-first rule:

```text
module query handler -> IApplicationCache -> HybridCache -> optional Redis L2
module command handler -> ICacheInvalidationQueue -> post-commit flush
```

Administration follows the same explicit front-door rule:

```text
Host.AdminCli -> *.AdminCli -> *.Application -> *.Domain
Host.AdminApi -> *.AdminApi -> *.Application -> *.Domain
                 |
                 -> *.Admin.Contracts
                 -> Shared.Administration contracts
```

## Why This Shape

The skeleton favors explicitness over magic. A module is optional only if the host has to opt into it. A boundary is real only if architecture tests and project references make it hard to accidentally cross it.
