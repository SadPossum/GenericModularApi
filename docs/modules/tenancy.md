# Tenancy Module

The Tenancy module enables tenant resolution for projects that need tenant isolation. It is optional and explicitly registered by `Host.Api`.

## Project

```text
Tenancy.Api
```

The module is intentionally small. Shared tenancy abstractions live in `Shared.Tenancy` and default/null implementations live in `Shared.Tenancy.Infrastructure`, which the core shared infrastructure composes by default.

## Runtime Behavior

When Tenancy is enabled:

- tenant id is resolved from `X-Tenant-Id`;
- tenant-scoped endpoints require a tenant id;
- tenant context is available through `ITenantContext`;
- tenant-scoped entities should include `TenantId`.

When Tenancy is not registered:

- shared infrastructure provides a null/default tenant context;
- non-tenant projects can still run.

## Endpoint

Base path:

```text
/api/tenants
```

Endpoint:

```text
GET /current
```

This returns the current tenant id and whether tenancy is enabled.

## Configuration

```json
{
  "Tenancy": {
    "Enabled": true,
    "HeaderName": "X-Tenant-Id",
    "LocalDefaultTenantId": "default"
  }
}
```

`HeaderName` is validated as an HTTP header token and tenant ids are normalized by `TenantIds`: trimmed, non-empty, case-preserving, at most 128 characters, and without whitespace or control characters to match module persistence mappings. `LocalDefaultTenantId` follows the same rule and is used by the shared null tenant context when the optional Tenancy module is not registered.

## Endpoint Usage

Tenant-scoped endpoints should call:

```csharp
.RequireTenant()
```

This applies the tenant endpoint filter from `Shared.Api`.

## Persistence Usage

Tenant-scoped entities should implement tenant-scoped behavior consistently:

- store a `TenantId`;
- use tenant-aware query filters;
- avoid bypassing filters without a specific reason;
- include tenant id in unique indexes where uniqueness is tenant-local.

## Testing

Any module with tenant-scoped data should prove:

- tenant A cannot see tenant B data;
- tenant A tokens cannot refresh or mutate tenant B sessions;
- missing tenant id is rejected where tenancy is required.

## Future Options

The current strategy is shared database tenancy. Future strategies can be added behind shared abstractions:

- separate schema per tenant;
- separate database per tenant;
- external tenant registry;
- tenant-specific feature flags.

Do not build those until a project needs them.
