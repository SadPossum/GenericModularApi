# GenericModularApi Documentation

This documentation is plain Markdown so it works in GitHub, Visual Studio, Rider, VS Code, and Obsidian. It uses an index-first structure rather than tool-specific vault configuration.

## Start Here

- [Setup](getting-started/setup.md)
- [Local Development](getting-started/local-development.md)
- [Architecture Overview](architecture/overview.md)
- [Development Guidelines](guidelines/development-guidelines.md)

## Architecture

- [Architecture Overview](architecture/overview.md)
- [Module System](architecture/module-system.md)
- [CQRS and Domain Events](architecture/cqrs-and-domain-events.md)
- [Persistence and Tenancy](architecture/persistence-and-tenancy.md)
- [Messaging and Outbox](architecture/messaging-and-outbox.md)
- [Messaging Consumers](architecture/messaging-consumers.md)
- [Module Descriptor Builder Refactor Notes](architecture/module-descriptor-builder-refactor-notes.md)
- [Observability](architecture/observability.md)
- [Caching](architecture/caching.md)
- [Administration](architecture/administration.md)
- [Tasks and Daemons](architecture/tasks-and-daemons.md)
- [Projection Rebuild Tasks](architecture/projection-rebuild-tasks.md)
- [Production Readiness Backlog](architecture/production-readiness-backlog.md)
- [Architecture Hardening Notes](architecture/audit-hardening-notes.md)
- [Architecture Audit Follow-Up Notes](architecture/audit-follow-up-notes.md)

## Existing Modules

- [Auth Module](modules/auth.md)
- [Tenancy Module](modules/tenancy.md)
- [Administration Module](modules/administration.md)

## Examples

- [Catalog Example Module](examples/catalog-module.md)
- [Ordering Example Module](examples/ordering-module.md)
- [Cross-Module Integration](examples/cross-module-integration.md)
- [TaskSamples Example Module](examples/task-samples-module.md)

## Guidelines

- [Naming Conventions](guidelines/naming-conventions.md)
- [Development Guidelines](guidelines/development-guidelines.md)
- [Testing Guidelines](guidelines/testing-guidelines.md)
- [Deployment Guidelines](guidelines/deployment-guidelines.md)
- [Documentation Guidelines](guidelines/documentation-guidelines.md)

## Templates

- [Module Documentation Template](templates/module.md)
- [ADR Template](templates/adr.md)
- [Integration Event Template](templates/integration-event.md)
- [Endpoint Template](templates/endpoint.md)

## ADRs

- [0001 Documentation Structure](adr/0001-documentation-structure.md)
- [0002 Explicit Optional Caching](adr/0002-explicit-optional-caching.md)
- [0003 Optional Administration CLI](adr/0003-optional-administration-cli.md)
- [0004 Optional Administration API](adr/0004-optional-administration-api.md)
- [0005 NATS Consumers and Cross-Module Data Ownership](adr/0005-nats-consumers-and-cross-module-data-ownership.md)
- [0006 Constrained Application Service Registration](adr/0006-constrained-application-service-registration.md)
- [0007 Shared CQRS Validation Contracts](adr/0007-shared-cqrs-validation-contracts.md)
- [0008 Optional Tasks and Daemons Foundation](adr/0008-optional-tasks-and-daemons-foundation.md)
- [0009 Configurable Application Identity](adr/0009-configurable-application-identity.md)

## Documentation Rules

- Keep docs close to implementation reality.
- Prefer short, linked pages over one huge document.
- Update module docs in the same change that changes module behavior.
- Use templates for new modules, ADRs, events, and endpoints.
