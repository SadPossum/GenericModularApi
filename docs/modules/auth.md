# Auth Module

The Auth module is a reusable first-party module. It proves the architecture end to end: endpoints, CQRS handlers, aggregate behavior, EF Core persistence, tenant isolation, JWT auth, refresh token hashing, domain events, outbox writing, and JetStream publishing.

## Projects

```text
Auth.Contracts
Auth.Domain
Auth.Application
Auth.Infrastructure
Auth.Infrastructure.JwtBearer
Auth.Persistence
Auth.Persistence.SqlServerMigrations
Auth.Persistence.PostgreSqlMigrations
Auth.Api
Auth.Admin.Contracts
Auth.Admin
Auth.AdminApi
```

## Public Endpoints

Base path:

```text
/api/auth
```

Endpoints:

- `POST /register`
- `POST /login`
- `POST /refresh`
- `POST /sign-out`
- `POST /sign-out-all`

Tenant-scoped endpoints require:

```http
X-Tenant-Id: <tenant-id>
```

Protected endpoints require:

```http
Authorization: Bearer <access-token>
```

## Contracts

`Auth.Contracts` contains:

- `Api/` self-service request/response records such as `RegisterMemberRequest`, `LoginMemberRequest`, `RefreshTokenRequest`, `SignOutRequest`, and `AuthTokensResponse`;
- `Admin/` admin member projection/response records used by CLI and admin HTTP flows;
- `Events/` integration event payloads and subject constants;
- `Metadata/` module metadata, permission code strings, and contract limits;
- `Types/` public enum-like contract types such as `UsernameType`.

These types are the public surface of the module.

Permission code strings live in `Auth.Contracts` for module metadata. Typed `AdminPermission` constants live in `Auth.Admin.Contracts` so public contracts do not reference the shared administration framework.

## Domain Model

Primary aggregate:

- `Member`

Supporting domain types:

- `MemberSession`
- `MemberUsername`
- `MemberId`
- `MemberSessionId`
- `MemberUsernameId`
- `MemberUsernameType`

Important domain behavior:

- create member;
- normalize usernames and keep username values within persistence limits;
- keep password hashes within persistence limits before member creation or reset;
- start session with a bounded refresh-token hash;
- refresh session with a bounded replacement refresh-token hash;
- sign out one session;
- sign out all sessions;
- disable member with a trimmed, 512-character maximum reason and revoke active sessions;
- enable disabled member;
- reset password;
- revoke active sessions as an admin action;
- raise member lifecycle domain events.

## Application Layer

Commands:

- `RegisterMemberCommand`
- `LoginMemberCommand`
- `RefreshMemberSessionCommand`
- `SignOutCommand`
- `SignOutAllCommand`
- `AdminCreateMemberCommand`
- `DisableMemberCommand`
- `EnableMemberCommand`
- `ResetMemberPasswordCommand`
- `RevokeMemberSessionsCommand`

Queries:

- `ListAdminMembersQuery`
- `GetAdminMemberQuery`

Handlers:

- create and authenticate members;
- rotate refresh tokens;
- verify session and tenant claims;
- project `MemberRegisteredDomainEvent` to the outbox.
- project member disabled, enabled, and session-revoked domain events to the outbox.

Validators:

- validate command shape before handler execution;
- keep endpoint handlers thin.

## Infrastructure

Auth infrastructure provides:

- validated issuer, audience, signing key, and access-token lifetime options;
- `PasswordHasher<T>` based password hashing;
- versioned HMAC-SHA256 refresh token hashing;
- access token generation and validation parameters.

Core Auth infrastructure and the JWT bearer adapter live in separate projects. CLI/admin-command hosts use `Auth.Infrastructure` and `services.AddAuthInfrastructure(configuration)` for hashing and token services without adding HTTP authentication schemes or ASP.NET Core bearer packages. HTTP Auth surfaces explicitly reference `Auth.Infrastructure.JwtBearer` and call `AddAuthJwtBearerAuthentication()` when they need bearer-token validation.

Auth application options validate `Auth:RefreshTokenLifetimeDays` so misconfigured refresh-token sessions fail at composition/startup instead of producing immediately expired or nonsensical sessions.

Refresh tokens are stored as hashes, never as raw token values.
The HMAC key is configured through `Auth:RefreshTokens:Pepper`. The option class intentionally has no secret default. Checked-in development settings provide a disposable local placeholder, and deployments must override it through a secret provider, for example `Auth__RefreshTokens__Pepper`.

JWT signing is configured through `Auth:Jwt`. The signing key must be at least 32 bytes and should also come from a secret provider outside local development.
Auth access tokens use `ClaimTypes.NameIdentifier` for the member id and shared `GmaClaimNames` constants for tenant and session claims. Keep claim-name changes centralized in `Shared.Application.Security.GmaClaimNames` so public Auth endpoints, admin APIs, token validation, and test token helpers stay aligned.

Login and refresh fail when a member is disabled.

## Persistence

Auth persistence owns:

- `AuthDbContext`
- EF configurations
- `MemberRepository`
- `AuthUnitOfWork`
- `AuthOutboxWriter`
- `AuthOutboxStore`
- admin member read projections
- provider-specific migrations

Auth uses schema:

```text
auth
```

Migration history table:

```text
auth.__ef_migrations_history
```

## Integration Events

Auth publishes:

```text
gma.auth.member-registered.v1
gma.auth.member-disabled.v1
gma.auth.member-enabled.v1
gma.auth.member-sessions-revoked.v1
```

Source domain event:

```text
MemberRegisteredDomainEvent
```

Public integration event:

```text
MemberRegisteredIntegrationEvent
MemberDisabledIntegrationEvent
MemberEnabledIntegrationEvent
MemberSessionsRevokedIntegrationEvent
```

## Admin Commands

`Auth.Admin` is optional and is composed by `Host.AdminCli`.
It shares typed permission constants with `Auth.AdminApi` through `Auth.Admin.Contracts`.

Commands:

- `auth members list --tenant <id> [--page] [--page-size] [--output table|json]`
- `auth members get --tenant <id> --member-id <id>`
- `auth members create --tenant <id> --username <value> --username-type email|phone`
- `auth members disable --tenant <id> --member-id <id> --reason <text> --yes`
- `auth members enable --tenant <id> --member-id <id>`
- `auth members reset-password --tenant <id> --member-id <id>`
- `auth members revoke-sessions --tenant <id> --member-id <id> --yes`

Permissions:

- `auth.members.read`
- `auth.members.create`
- `auth.members.disable`
- `auth.members.enable`
- `auth.members.reset-password`
- `auth.members.revoke-sessions`

Password input supports hidden prompt, `--password-stdin`, or `--generate-password`. There is no `--password` option. Generated passwords are printed once after a successful command and must not be logged or audited.

## Admin API

`Auth.AdminApi` is optional and is composed by `Host.AdminApi`.

Routes:

- `GET /api/admin/auth/members`
- `GET /api/admin/auth/members/{memberId}`
- `POST /api/admin/auth/members`
- `POST /api/admin/auth/members/{memberId}/disable`
- `POST /api/admin/auth/members/{memberId}/enable`
- `POST /api/admin/auth/members/{memberId}/reset-password`
- `POST /api/admin/auth/members/{memberId}/revoke-sessions`

Tenant-scoped routes require `X-Tenant-Id`. Destructive routes require an explicit `confirmed: true` request body field. Generated passwords are returned once and must not be logged or audited.

## Tests

Relevant test groups:

- `Auth.Tests` for aggregate and unit-of-work behavior.
- `Integration.Tests` for lifecycle, tenant isolation, outbox publishing, and outbox store behavior.
- `Architecture.Tests` for module boundaries.

## Extension Points

Likely future changes:

- add email verification;
- add password reset;
- add member profile module that consumes Auth contracts/events;
- replace first-party auth infrastructure with external identity provider adapter while keeping contracts stable.

Keep the module reusable. Do not tie Auth to a specific product domain.
