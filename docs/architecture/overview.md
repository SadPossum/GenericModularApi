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
    Shared.Application/
    Shared.Caching.Redis/
    Shared.Domain/
    Shared.ErrorHandling/
    Shared.Infrastructure/
    Shared.Logging.Serilog/
    Shared.Messaging.Nats.Aspire/
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
    Tenancy/
      Tenancy.Api/
tests/
  Shared.Tests/
  Auth.Tests/
  Architecture.Tests/
  Integration.Tests/
```

## Runtime Composition

`Host.Api` is the composition root:

1. Adds optional infrastructure adapters such as Redis, OpenAPI, and configured NATS publishing.
2. Adds shared infrastructure.
3. Optionally adds NATS and JetStream messaging.
4. Explicitly registers optional modules.
5. Maps module endpoints.

Current host registration:

```csharp
builder.AddRedisCaching(); // no-op unless Redis caching is enabled
builder.AddSharedInfrastructure();
builder.AddConfiguredNatsJetStreamMessaging(); // no-op unless NATS publishing is enabled
builder.Services.AddGmaApiSecurityDefaults(); // no default scheme; Auth or another adapter supplies one
builder.AddModule<TenancyModule>();
builder.AddModule<AuthModule>();
builder.AddGmaOpenApi();
app.UseGmaOpenApi(); // serves Swagger only in Development
app.MapModules();
```

`Host.AdminCli` is a separate optional composition root for administrative commands:

```csharp
builder.AddSharedAdministrationCli();
builder.AddSharedInfrastructure();
builder.AddAdminModule<AdministrationAdminCliModule>();
builder.AddAdminModule<AuthAdminCliModule>();
```

It does not map public API endpoints.

`Host.AdminApi` is a separate optional composition root for administrative HTTP APIs:

```csharp
builder.Services.AddSharedAdministrationApi(builder.Configuration);
builder.AddSharedInfrastructure();
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

Shared.Infrastructure
  -> Shared.Application
  -> Shared.Domain
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
