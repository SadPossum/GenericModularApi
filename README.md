# GenericModularApi

GenericModularApi is a .NET 10 modular monolith skeleton for building projects from optional, replaceable modules.

The repo is intentionally small and explicit:

- modules are registered by the host, not discovered by assembly scanning;
- cross-module communication goes through contracts and integration events;
- EF Core is the practical unit of work;
- tenant support starts with shared-database isolation through `TenantId`;
- reliable cross-boundary publishing goes through outbox tables and a NATS JetStream adapter.
- optional cache-aside reads use provider-neutral contracts, HybridCache, and an opt-in Redis adapter.
- optional administration uses a separate CLI host, persisted RBAC/audit, and feature-owned admin front doors.
- optional admin HTTP APIs use a separate `Host.AdminApi` composition root.

## Quick Start

```powershell
.\eng\restore.ps1
.\eng\build.ps1 -NoRestore
.\eng\test-fast.ps1 -NoBuild
```

Run the full local development stack with Aspire:

```powershell
.\eng\run-aspire.ps1
```

Run only the API:

```powershell
.\eng\run-api.ps1
```

Run the optional admin CLI locally:

```powershell
.\eng\run-admin.ps1 -- admin roles list --actor owner
```

Run the optional admin API locally:

```powershell
.\eng\run-admin-api.ps1
```

Docker-backed tests are skippable by default. To require them:

```powershell
.\eng\test-docker.ps1 -NoBuild
```

## Documentation

Start with [docs/README.md](docs/README.md).

Useful entry points:

- [Setup](docs/getting-started/setup.md)
- [Architecture Overview](docs/architecture/overview.md)
- [Module System](docs/architecture/module-system.md)
- [Administration](docs/architecture/administration.md)
- [Auth Module](docs/modules/auth.md)
- [Administration Module](docs/modules/administration.md)
- [Tenancy Module](docs/modules/tenancy.md)
- [Naming Conventions](docs/guidelines/naming-conventions.md)
- [Development Guidelines](docs/guidelines/development-guidelines.md)
- [Documentation Guidelines](docs/guidelines/documentation-guidelines.md)

## Request Examples

HTTP examples live in [requests/auth.http](requests/auth.http) and [requests/admin-api.http](requests/admin-api.http).

## Project Status

This is a work-in-progress skeleton. The current reusable modules are Auth, Tenancy, and optional Administration.
Catalog and Ordering are compiled optional example modules used to prove stored data, admin surfaces, caching, and cross-module integration without being registered in default hosts.
