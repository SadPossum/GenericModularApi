# Shared Access Policy Foundation

## Decision

This is worth doing as a small shared foundation, but not as a full authorization product.

Add shared access-policy vocabulary and contracts so modules can express resource authorization consistently. Do not replace the current Administration RBAC system, do not add a generic ACL table in the first slice, and do not hide access checks behind broad endpoint or CQRS magic.

The goal is a stable place for access-control language:

- who is asking;
- what action they want;
- what resource or requirement is being checked;
- whether the decision is allowed or denied;
- how modules should enforce that decision on single-resource and list/read-model paths.

The business meaning of access still belongs to the module that owns the resource.

## Task Text

Implement a small `Shared.AccessControl` foundation for resource-level access decisions.

The first implementation should provide backend-agnostic contracts and value objects for access subjects, actions, resources, requirements, and decisions. Feature modules should be able to define resource-specific requirements such as `ViewPostRequirement`, `EditReportRequirement`, or `ManageStaffRecordRequirement`, then implement explicit policies for those requirements.

Keep existing admin RBAC unchanged. `Shared.Administration` and the optional `Administration` module remain responsible for admin operation authorization, tenant-scoped role assignments, and admin audit. The new access-policy foundation is for product/resource authorization such as ownership, friendship, team membership, document sharing, staff hierarchy, and object-level visibility.

Do not add persisted grants, generic groups, custom policy languages, external policy engines, or automatic query filtering in the first slice. Those can be added later behind the same contracts after at least one real module proves the shape.

## Problem

The skeleton currently has strong admin-operation authorization and tenant isolation, but it does not have a general vocabulary for resource access.

That is fine for simple modules:

- a tenant member can list tenant orders;
- an admin actor can run `auth.members.read`;
- a catalog item is tenant-scoped and visible through the tenant filter.

It becomes too vague for applications such as:

- social networks where friends, followers, blocked users, and custom lists affect post visibility;
- document/report tools where users, groups, teams, and admins can receive viewer/editor/owner access;
- staff management tools where managers can see direct reports, HR can see broader data, and sensitive fields require extra authority;
- collaboration products where access is inherited from workspaces, folders, projects, or organizations.

Without a shared policy vocabulary, every module will invent its own subject/action/decision types, denial behavior, and test shape. That makes access-control bugs easier to introduce and harder to audit.

## Current RBAC Boundary

Current admin RBAC should stay.

Admin RBAC answers:

```text
Can this admin actor perform this admin operation?
```

Examples:

```text
auth.members.read
catalog.items.update
admin.roles.manage
tasks.runs.cancel
```

Resource policy answers:

```text
Can this subject perform this action on this resource?
```

Examples:

```text
Can user-123 view post-456?
Can admin-7 edit report-9?
Can manager-4 see compensation fields for employee-5?
Can service billing-worker close invoice-8?
```

These are related but not interchangeable. An admin may be allowed to open a backoffice page through RBAC and still need resource policy checks inside the module when the page touches object-level data. Conversely, a normal product user may have resource access without any admin RBAC assignment.

## Design Principles

- Deny by default.
- Keep the shared package backend-agnostic.
- Keep resource rules module-owned.
- Make checks explicit at command/query boundaries.
- Enforce access in read repositories and projections, not only in HTTP endpoints.
- Avoid per-row policy checks for list, feed, search, and export paths.
- Keep tenant handling visible in the subject, resource, or requirement.
- Keep denial reasons stable and non-sensitive.
- Keep admin RBAC and resource policy separate unless an explicit adapter is added later.
- Prefer narrow primitives over a generic persisted sharing system until a real module proves the need.

## Non-Goals

- Do not replace `Shared.Administration`, `AdminApiExecutor`, `IAdminOperationRunner`, or persisted Administration RBAC.
- Do not move feature-specific rules such as `friend`, `blocked`, `owner`, `manager`, `hr`, `viewer`, or `editor` into shared code.
- Do not add a generic `access_grants` table in the first slice.
- Do not adopt OPA, Cedar, OpenFGA, SpiceDB, or another external policy engine in the first slice.
- Do not add automatic endpoint filters that make authorization invisible.
- Do not add a generic EF query-filter builder for all modules.
- Do not tag metrics with tenant ids, user ids, resource ids, or policy input values.

## Proposed Contracts

The first shared package should be contract-only:

```text
src/Shared/Shared.AccessControl
```

Suggested primitives:

```csharp
public enum AccessSubjectKind
{
    Unknown = 0,
    User = 1,
    AdminActor = 2,
    Service = 3,
    System = 4
}

public sealed record AccessSubject(
    AccessSubjectKind Kind,
    string Id,
    string? TenantId);

public sealed record AccessAction(string Code);

public sealed record AccessResourceRef(
    string Module,
    string ResourceType,
    string ResourceId,
    string? TenantId);

public interface IAccessRequirement
{
    AccessAction Action { get; }
    AccessResourceRef? Resource { get; }
}

public sealed record AccessDecision
{
    public static AccessDecision Allow() => new(true, null);
    public static AccessDecision Deny(string reasonCode = AccessDecisionReasonCodes.NotAuthorized) =>
        new(false, reasonCode);

    private AccessDecision(bool allowed, string? reasonCode)
    {
        this.Allowed = allowed;
        this.ReasonCode = reasonCode;
    }

    public bool Allowed { get; }
    public string? ReasonCode { get; }
}

public interface IAccessPolicy<in TRequirement>
    where TRequirement : IAccessRequirement
{
    ValueTask<AccessDecision> AuthorizeAsync(
        AccessSubject subject,
        TRequirement requirement,
        CancellationToken cancellationToken);
}
```

The exact names can change during implementation, but the shape should stay small: subject, action, resource reference, requirement, decision, policy.

## Subject Resolution

`Shared.AccessControl` should not reference ASP.NET Core, Auth, Administration, EF, or a specific identity provider.

Front doors and application code should construct `AccessSubject` explicitly:

- public API: authenticated member id plus active tenant id;
- admin API/CLI: current `AdminActor` id plus operation tenant when present;
- workers: service or system subject with bounded id;
- tests: deterministic test subject factories.

An optional adapter can be added later, for example:

```text
Shared.AccessControl.Api
Shared.AccessControl.Administration
```

Do not put claims parsing or HTTP concerns in the core contracts package.

## Module-Owned Policies

Feature modules own their requirements and policy implementations.

Social example:

```csharp
public sealed record ViewPostRequirement(Guid PostId)
    : IAccessRequirement;
```

The Posts module owns the post visibility rules. The Friendships module owns friendship/block relationships. The Posts read side may use a Friendships read contract, a local friendship projection, or a dedicated feed/read model depending on performance needs.

Staff management example:

```csharp
public sealed record ViewStaffProfileRequirement(
    Guid EmployeeId,
    bool IncludeSensitiveFields)
    : IAccessRequirement;
```

The Staff module owns what manager, HR, employee self-service, sensitive fields, and terminated employee data mean. Shared code should not know those concepts.

Reports/documents example:

```csharp
public sealed record EditReportRequirement(Guid ReportId)
    : IAccessRequirement;
```

If many modules need the same viewer/editor/owner grant model, add a future optional persisted `AccessControl` module. Do not guess that model before repeated product needs exist.

## Enforcement Points

Single-resource commands and queries can call a policy directly:

```text
load resource summary -> authorize -> execute or return access denied
```

List/search/feed/export paths must not call a policy once per row after loading broad data. They should enforce visibility inside the repository, projection, query, or read model:

```text
current subject + query filters -> policy-aware read model -> already-visible rows
```

Examples:

- friends-only posts should use friendship joins, local projections, or feed materialization;
- report lists should join against grants or project visible report ids;
- staff search should pre-filter by managed departments or HR scope;
- exports should use the same visibility filter as the interactive list endpoint.

Every module that introduces resource policy must document which paths enforce it:

- get by id;
- list/search;
- create;
- update/delete;
- export/background jobs;
- notification or live-stream delivery;
- admin read and write paths.

## Tenant Handling

Access checks must preserve tenant boundaries.

Rules:

- `AccessSubject.TenantId` should be set when the caller is acting inside a tenant.
- `AccessResourceRef.TenantId` should be set for tenant-owned resources.
- Policies must deny tenant mismatch unless the requirement explicitly models a global/platform resource.
- Admin RBAC may authorize a tenant-scoped operation, but the module still needs resource policy when object-level visibility matters.
- Background jobs must pass a service/system subject plus the tenant/resource context they are acting on.

Tenancy remains separate from resource authorization. Tenant filters prevent cross-tenant reads; resource policies decide access inside the tenant or across explicitly global resources.

## Relationship To Administration RBAC

Keep the existing Administration flow unchanged:

```text
AdminApiExecutor/AdminCliExecutor
  -> IAdminOperationRunner
  -> IAdminAuthorizationService
  -> audit
  -> module command/query
```

Possible future bridge:

```csharp
public sealed record AdminOperationAccessRequirement(
    AdminPermission Permission,
    string? TenantId)
    : IAccessRequirement;
```

An adapter could wrap `IAdminAuthorizationService` and return `AccessDecision`, but this is optional and should not replace the current admin operation runner. The runner owns production behavior that generic policy code should not duplicate: tenant setup, audit, confirmation-friendly execution flow, shaped failures, and logging-failure resilience.

## Future Persisted Grants Module

Add a persisted `AccessControl` module only when several modules need the same object-sharing model.

Possible future model:

```text
subject kind/id/tenant
resource module/type/id/tenant
relation or level
created by/at
expires at
source
```

Example relations:

```text
owner
viewer
editor
commenter
member
manager
```

This should be optional and explicit. Many products do not need it, and many domains have access rules that cannot be reduced cleanly to generic grants.

## External Policy Engines

The shared foundation should make future adapters possible without choosing one now.

Possible later adapters:

- OPA/Rego for policy-as-code;
- Cedar/Amazon Verified Permissions style PARX decisions;
- OpenFGA or SpiceDB for Zanzibar-style relationship tuples;
- custom in-process policy modules for a modular monolith.

Do not take a dependency on any of these in the skeleton until there is a real deployment need.

## Functional Requirements

- Add `Shared.AccessControl` as a small contracts project.
- Normalize subject ids, action codes, resource module/type names, reason codes, and tenant ids through shared naming/value helpers where possible.
- Decision creation must make allow/deny explicit.
- Unknown subject kind, empty subject id, invalid action code, invalid resource ref, or invalid reason code must fail early.
- Policies must be registered explicitly by the owning module.
- Modules must define typed requirement records for resource-specific checks.
- Public API, admin API, CLI, and workers must construct subjects explicitly.
- Access-denied outcomes should map to `403` for authenticated callers and `401` only when no trustworthy identity exists.
- Modules should avoid returning different not-found versus forbidden details when that would reveal private resource existence.
- Read/list/export paths must have policy-aware data access instead of after-the-fact filtering.
- Tests must prove deny-by-default behavior before allowing new access rules.

## Non-Functional Requirements

### Security

- Deny by default.
- Do not leak private resource existence through detailed denial messages.
- Do not store secrets, claims, raw tokens, or large policy inputs in decisions.
- Do not put tenant ids, user ids, resource ids, or subject ids into metric tags.
- Avoid caching allow decisions unless the module has a documented invalidation story.
- If policy input includes mutable relationships, tests must cover revocation.

### Performance

- No N+1 policy checks for lists, feeds, exports, or streams.
- Prefer policy-aware SQL, local projections, precomputed feed tables, or bounded batch checks.
- If a module uses batch authorization, result ordering and missing-resource behavior must be deterministic.
- Expensive relationship checks should expose a read-optimized port or projection.

### Observability

- Log policy failures only with bounded reason codes and module/action/resource type.
- Avoid logging subject ids or resource ids unless the module has an audit requirement and a retention policy.
- Access-control metrics should be bounded by module, action, resource type, and decision.
- Admin audit remains in Administration; product/resource audit should be module-owned unless a future shared audit package is introduced.

### Maintainability

- Keep requirements small and named after the business operation.
- Keep shared contracts independent from HTTP, EF, Auth, Administration, and external engines.
- Do not infer policies through reflection or attributes in the first slice.
- Architecture tests should guard project boundaries and ensure modules do not reference access adapters from domain projects.

## Testing Requirements

- Unit tests for value-object normalization and rejection.
- Unit tests for `AccessDecision` factories.
- Module tests for each concrete policy.
- Integration tests for at least one single-resource path and one list path when a module adopts policies.
- Tenant-mismatch tests for tenant-owned resources.
- Revocation tests when access depends on mutable relationships or grants.
- Architecture tests that keep `Shared.AccessControl` backend-free.
- Architecture tests that keep resource policy implementations out of domain projects.
- Regression tests for not-found versus forbidden behavior on private resources.

## Acceptance Criteria

- `Shared.AccessControl` compiles as a backend-agnostic contracts package.
- Existing admin RBAC behavior is unchanged.
- Documentation explains that admin RBAC and resource policy solve different problems.
- At least one sample or test-only policy demonstrates explicit requirement registration and evaluation.
- The implementation includes guard tests for invalid subjects, actions, resources, and decision reasons.
- The implementation includes guidance or tests preventing list endpoints from loading broad data and filtering only in memory.
- No new persisted ACL/grants schema is added in the first slice.
- No external policy engine dependency is added in the first slice.

## Implementation Slices

1. Contracts foundation
   - Add `Shared.AccessControl`.
   - Add value objects, decision factories, interfaces, and tests.
   - Update naming/development docs and architecture catalog tests.

2. Policy registration
   - Add explicit `AddAccessPolicy<TRequirement,TPolicy>()` registration helper if repeated registration becomes noisy.
   - Keep registration explicit and module-owned.

3. First module adoption
   - Pick one real module with object-level visibility.
   - Add resource-specific requirements and policies.
   - Prove both get-by-id and list/read-model enforcement.

4. Admin bridge, optional
   - Add an adapter from admin RBAC to `AccessDecision` only if it removes duplication.
   - Do not replace `IAdminOperationRunner`.

5. Persisted grants, optional
   - Add a separate optional `AccessControl` module only after multiple modules need the same grant/level model.
   - Include migrations, admin management APIs, revocation behavior, cache invalidation rules, and audit/observability requirements.

6. External policy engine adapter, optional
   - Add OPA, Cedar, OpenFGA, SpiceDB, or another adapter only for a concrete deployment need.
   - Keep adapters outside the core contracts project.

## Reference Points

These are reference points for terminology and production concerns, not dependencies:

- [OWASP Authorization Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html)
- [NIST SP 800-162 Attribute Based Access Control](https://csrc.nist.gov/pubs/sp/800/162/upd2/final)
- [ASP.NET Core resource-based authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resource-based)
- [Zanzibar: Google's Consistent, Global Authorization System](https://research.google/pubs/zanzibar-googles-consistent-global-authorization-system/)
