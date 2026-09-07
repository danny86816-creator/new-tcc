# ADR-0002: Tcc.Features.Themes Ownership Boundary

**Status:** Accepted  
**Decision date:** 2026-09-07  
**Decision authority:** Explicit human architecture decision  

## Context

The approved Theme Architecture proposed `Tcc.Features.Themes`, while the previously frozen physical repository structure did not list that package. The proposal required explicit architecture approval before creation.

## Decision

Create `Tcc.Features.Themes` as the presentation feature owner for Theme management experiences.

Its authority is limited to:

- Theme Library user interface.
- Theme Preview user interface.
- Theme Personalization user interface.
- Theme-management presentation orchestration.

The core Theme runtime remains owned by `Tcc.Themes`.

`Tcc.Features.Themes` may depend on `Tcc.Themes` and `Tcc.Presentation.Contracts`. A later phase may add a dependency on approved application command/query abstractions when required for presentation orchestration. It must not depend on concrete application handlers or implementations.

## Prohibited ownership and access

`Tcc.Features.Themes` must not own or directly access:

- Theme engine/runtime internals.
- Domain rules or aggregates.
- Persistence implementations or direct database access.
- Manifest, integrity, compatibility, capability, archive, or security validation cores.
- Recovery Core or Safe Mode ownership.
- Connector APIs.
- AI providers or AI workflow authority.
- Credentials or secrets.
- Authoritative trading data mutation.

It must not bypass App Lock, permission, confirmation, audit, accessibility, Theme validation, or recovery boundaries.

## Dependency direction

```text
Tcc.DesktopHost
  -> Tcc.Features.Themes
       -> Tcc.Themes
       -> Tcc.Presentation.Contracts

Tcc.Themes
  -> Tcc.Presentation.Contracts
```

There must be no reverse dependency from `Tcc.Themes` to `Tcc.Features.Themes`.

## Consequences

- Theme management views receive an explicit WPF/MVVM package boundary.
- Presentation changes can evolve without moving Theme engine policy into the feature assembly.
- Architecture tests must prevent direct references to Domain, Persistence, Connector, AI, Security, or Recovery implementations.
- `Tcc.DesktopHost` remains responsible for dependency injection and composition.

## Phase 1 boundary

Phase 1 creates only the buildable project boundary and assembly marker. Theme Library, Preview, Personalization, and orchestration implementations remain deferred to their approved later phase.
