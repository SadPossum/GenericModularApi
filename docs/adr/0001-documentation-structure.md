# ADR 0001: Documentation Structure

## Status

Accepted

## Date

2026-06-15

## Context

The project is a modular monolith skeleton intended to be reused across future projects. It needs documentation that helps with onboarding, module authoring, naming, deployment, and architectural consistency.

The docs should be easy to read in common developer tools and should not depend on a proprietary or local-only documentation system.

## Decision

Use plain Markdown under `docs/` with this structure:

```text
docs/
  README.md
  getting-started/
  architecture/
  modules/
  guidelines/
  templates/
  adr/
```

The docs are compatible with Obsidian, but the repo does not commit `.obsidian` configuration or plugin requirements.

## Consequences

Positive:

- works in GitHub, IDEs, and Obsidian;
- keeps docs versioned with code;
- keeps architecture and module docs easy to find;
- supports repeatable templates for new modules and decisions.

Negative:

- diagrams are limited to plain Markdown or Mermaid;
- no generated documentation site until one is needed.

## Alternatives Considered

- Obsidian-specific vault: good personal navigation, but adds editor-specific state to the repo.
- Static docs site: useful later, but too much infrastructure for the current skeleton.
- Single huge README: simple at first, but harder to maintain as modules grow.
