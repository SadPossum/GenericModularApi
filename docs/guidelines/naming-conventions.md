# Naming Conventions

Use concise names that match project names. Do not root-prefix namespaces with `GenericModularApi`.

## Namespaces

Namespace starts with the owning project name:

```text
Auth.Application
Auth.Domain.Aggregates
Shared.Messaging.Infrastructure
Tenancy.Api
```

Do not use:

```text
GenericModularApi.Auth.Application
GenericModularApi.Modules.Auth.Application
```

Architecture tests enforce this convention for source files.

## Projects

Every `.csproj` under `src/` and `tests/` should live in a folder with the same name as the project file:

```text
src/Modules/Auth/Auth.Application/Auth.Application.csproj
tests/Auth.Tests/Auth.Tests.csproj
```

This keeps project references, namespaces, solution folders, and file-system navigation aligned.

Do not set `<RootNamespace>` or `<AssemblyName>` in project files unless a separate architecture decision explains the exception. The default SDK behavior keeps assembly names, root namespaces, project files, and folders aligned.

Module project names:

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

Shared project names:

```text
Shared.Api
Shared.Administration
Shared.Administration.Api
Shared.Administration.Cli
Shared.Application.Composition
Shared.Application.Events
Shared.Application.Events.Infrastructure
Shared.Api.OpenApi
Shared.Api.Serilog
Shared.Authorization
Shared.Caching
Shared.Caching.Cqrs
Shared.Caching.Infrastructure
Shared.Caching.Redis
Shared.Cqrs
Shared.Cqrs.Infrastructure
Shared.Domain
Shared.Results
Shared.Infrastructure
Shared.Logging.Serilog
Shared.Messaging
Shared.Messaging.Infrastructure
Shared.Messaging.Nats
Shared.Messaging.Nats.Aspire
Shared.Modules
Shared.Naming
Shared.Numerics
Shared.Observability
Shared.Observability.Infrastructure
Shared.Pagination
Shared.Persistence.EntityFrameworkCore
Shared.ProjectionRebuild
Shared.ProjectionRebuild.EntityFrameworkCore
Shared.ProjectionRebuild.Tasks
Shared.Runtime
Shared.Runtime.Infrastructure
Shared.Security
Shared.Tasks
Shared.Tasks.Cqrs
Shared.Tasks.Infrastructure
Shared.Tenancy
Shared.Tenancy.Infrastructure
```

## Folders

Recommended module folders:

```text
Commands/
Handlers/
Validation/
Aggregates/
Entities/
Events/
ValueObjects/
Repositories/
Services/
Configurations/
Migrations/
```

Do not create folders before they help navigation.

## Commands and Queries

Commands:

```text
RegisterMemberCommand
SignOutAllCommand
```

Handlers:

```text
RegisterMemberCommandHandler
SignOutAllCommandHandler
```

Keep one handler class per file under `Handlers/`. The file name should match the handler class name, including domain-event projectors and integration-event handlers.

Validators:

```text
RegisterMemberCommandValidator
```

Queries:

```text
GetCurrentMemberQuery
```

Query handlers:

```text
GetCurrentMemberQueryHandler
```

Commands that mutate persistent module state use:

```text
ITransactionalCommand<TResponse>
```

Plain `ICommand<TResponse>` is reserved for command-like operations that do not need a module EF commit.

Module-owned persistence services expose lowercase kebab-case module names:

```text
auth
catalog
ordering
administration
customer-support
```

## Events

Domain events:

```text
MemberRegisteredDomainEvent
```

Integration events:

```text
MemberRegisteredIntegrationEvent
```

Keep one public contract type per file in `.Contracts`, including DTOs, enums, permission code containers, and integration events.

Module metadata lives in `<Module>.Contracts` when it is public to tests, docs, or other modules. Use `Name` for the lowercase module identity and only set `AdminSurfaceName` when the public admin command/permission prefix intentionally differs from the module name.
Descriptor constructor parameters use normal C# camelCase even when they initialize PascalCase properties; named arguments in module metadata should follow the constructor parameter names.

## Enums

Public contract enums, public domain-state enums, and provider/configuration enums that select infrastructure backends use `Unknown = 0`:

```text
Unknown = 0
Active = 1
Disabled = 2
```

Keep existing numeric values stable once the enum is persisted, published in an API/event contract, or bound from configuration. Handlers and option validators should reject unsupported input values explicitly; mapping code must not collapse unknown values into meaningful business states such as `Active` or real providers such as `SqlServer`, `Memory`, or `Redis`.

Subjects:

```text
{application-namespace}.{module}.{event}.v{version}
```

Example:

```text
gma.auth.member-registered.v1
```

Use lowercase kebab-case for the `{application-namespace}`, `{module}`, `{event}`, and integration-event consumer handler-name segments. The default application namespace is `gma`; set `ApplicationIdentity:Namespace` to the product or bounded system name before creating production NATS streams, cache keys, or dashboards. Module contracts should expose stable logical event names and subject factory methods rather than embedding physical provider subjects by hand. Subscription metadata is validated at composition time, so invalid subject shapes such as extra dots, spaces, or zero-padded versions should fail before a host starts consumers.

## Endpoints

Use lowercase kebab-case paths:

```text
/api/auth/sign-out-all
/api/tenants/current
```

Use PascalCase tags:

```text
Auth
Tenancy
```

## Admin Commands

Use lowercase command names and kebab-case options:

```text
auth members reset-password --member-id <id>
admin roles grant --permission auth.members.read
```

Admin operation names and admin permission codes use dotted lowercase names:

```text
<module>.<resource>.<action>
```

Examples:

```text
auth.members.create
auth.members.read
auth.members.reset-password
admin.roles.manage
```

Admin role names use lowercase slug names. Use letters, numbers, and hyphens only, starting with a letter:

```text
owner
support
tenant-operator
```

## Tests

Test classes that contain `[Fact]`, `[Theory]`, or `[DockerFact]` must end with `Tests`.

Examples:

```text
MemberAggregateTests
AuthLifecycleIntegrationTests
ModuleBoundaryTests
```

Test method names should describe behavior:

```text
Members_and_sessions_are_isolated_by_tenant
Register_login_refresh_and_sign_out_runs_against_sql_server_and_postgre_sql
```

## Scripts

Scripts live in `eng/` and use kebab-case:

```text
test-fast.ps1
test-docker.ps1
add-migration.ps1
new-module.ps1
```
