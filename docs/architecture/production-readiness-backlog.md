# Production Readiness Backlog

This tracker records the long-running hardening backlog for the skeleton. Each item should end as either implemented with tests/docs or deliberately excluded with documented reasoning.

## Current Principles

- Preserve the small-core modular monolith direction.
- Keep modules optional and explicitly composed.
- Prefer contracts, events, local projections, and replaceable adapters over cross-module internals.
- Add magic only when it reduces meaningful maintenance cost and is guarded by tests/docs.
- Keep production-readiness work evidence-backed: architecture guards, targeted tests, docs, and scripts should move together.

## Backlog

| Item | Status | Notes |
| --- | --- | --- |
| Contracts and folder structure | In progress | Public contracts now use `Api/`, `Admin/`, `Events/`, `Metadata/`, and `Types/`. Admin contract wrappers use `Permissions/` and `Operations/`. |
| Shared event abstractions | Implemented | Added `IntegrationEvent`, `DomainEvent`, and `TenantDomainEvent` base records, migrated Auth/Catalog events, and guarded module events from bypassing shared metadata validation. |
| Admin naming | Implemented | CLI-only module front doors use `.AdminCli`; shared typed permission/operation helpers stay in `.Admin.Contracts`, and HTTP admin front doors stay in `.AdminApi`. |
| Test organization and value audit | Implemented | Test files now live under intent folders, docs describe the taxonomy, and architecture guards enforce test categories, names, Docker traits, and folder placement. Follow-up notes capture the remaining long-term split and coverage watchpoints. |
| Code magic/reflection | Pending | Evaluate constrained assembly registration for handlers/validators; document why built-in .NET alternatives are or are not enough. |
| Validation library | Pending | Evaluate FluentValidation against the current small validation contracts; switch only if consistency and ergonomics improve. |
| Tasks/daemons framework | Pending | Design optional production-ready task runtime with monitoring, control, node placement, and command/control communication. |
| Notifications and streaming | Pending | Design optional user notification and real-time update module/adapters. |
| File storage | Pending | Design optional file storage contracts and first real adapter, likely MinIO/S3-compatible if it fits. |
| External auth | Pending | Add provider-based login/registration, account linking, and migration paths while keeping Auth reusable. |
