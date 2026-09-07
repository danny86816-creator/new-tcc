# ADR-0001: TCC Windows Desktop Technology Stack

**Status:** Accepted  
**Decision date:** 2026-09-07  
**Decision authority:** Explicit human architecture decision  

## Context

The frozen system architecture intentionally left the programming language, desktop UI framework, persistence technology, test framework, and distribution mechanism open. Phase 1 requires a concrete, buildable foundation without implementing Theme Platform behavior ahead of its approved phases.

TCC is Windows-first, local-first, accessibility-sensitive, and expected to support native desktop behaviors including WPF presentation, Windows x64 packaging, Installer delivery, and a self-contained Portable delivery mode.

## Decision

TCC will use the following platform stack:

| Area | Decision |
|---|---|
| Language and runtime | C# on .NET 10 LTS |
| Desktop UI | WPF targeting `net10.0-windows` |
| Presentation pattern | MVVM |
| Process composition | `Microsoft.Extensions.Hosting` Generic Host and Microsoft dependency injection |
| Local persistence | SQLite through Entity Framework Core |
| Automated tests | xUnit |
| Supported architecture | Windows x64 |
| Distribution | Installer and self-contained Portable distributions |

The repository uses `global.json` with a .NET 10 minimum SDK feature band and roll-forward within .NET 10. All Phase 1 projects target Windows x64. WPF belongs only in the desktop host and presentation feature assemblies; business and platform boundaries must not be moved into UI code.

The desktop host is the composition root. It may bind interfaces to concrete implementations through dependency injection. Feature and runtime packages must not become alternate composition roots.

MVVM is an architectural pattern decision, not approval of a third-party MVVM framework. A future proposal to introduce an MVVM toolkit requires a separate dependency decision.

SQLite and EF Core are approved for the future persistence layer, but Phase 1 does not create an EF model, database, migration, or persistence implementation. EF entities and mappings must remain outside `Tcc.Domain`, and Theme metadata must remain platform metadata owned through the approved Theme boundaries.

Installer and Portable use the same application behavior and contracts. Portable publication must be self-contained for `win-x64` and must not create a mandatory machine-wide installation or registry dependency. The exact installer technology remains a later, separately reviewed implementation decision.

## Rationale

- .NET 10 is the approved LTS generation and provides a supported Windows desktop runtime baseline.
- C# and WPF provide direct access to the Windows desktop presentation and accessibility ecosystem required by the product architecture.
- MVVM keeps view state and presentation orchestration testable without putting product rules in code-behind.
- Generic Host provides one composition, lifetime, configuration, logging, and dependency-injection model for the desktop process.
- SQLite fits the local-first, offline-capable single-device store, while EF Core provides explicit mappings and versioned migrations.
- xUnit provides a small automated test runner suitable for unit and architecture-boundary tests.
- A fixed x64 target reduces ambiguity for WPF, native SQLite assets, packaging, diagnostics, and release qualification.
- Installer plus self-contained Portable delivery preserves the approved dual-distribution requirement while keeping core behavior identical.

## Constraints

- The UI is Windows-specific; no cross-platform UI support is implied.
- Projects compile for x64 and are not qualified for x86, Arm64, or AnyCPU.
- WPF code must not own Domain, risk, permission, audit, persistence, security, recovery, connector, AI, or Theme engine policy.
- `Tcc.DesktopHost` is the only application composition root.
- EF Core migrations must be explicit, reviewable, reversible where feasible, and tested against backup/recovery rules.
- SQLite write coordination must follow the frozen single-writer and transaction boundaries.
- Portable mode must resolve paths relative to its approved data root and preserve feature parity with Installer mode.
- Self-contained distribution increases artifact size and servicing responsibility.
- The exact installer builder, signing pipeline, MVVM helper library, SQLite protection method, and release publishing profiles are not decided by this ADR.

## Alternatives considered

| Alternative | Disposition |
|---|---|
| .NET 8 LTS | Not selected; the approved decision establishes .NET 10 as the new project baseline. |
| WinUI 3 | Not selected; the approved desktop UI decision is WPF. |
| Avalonia | Not selected; cross-platform UI is outside the current Windows-first decision. |
| Electron/TypeScript | Not selected; it conflicts with the approved C#/.NET/WPF stack. |
| .NET MAUI | Not selected; mobile and multi-platform UI are not part of this desktop foundation. |
| Raw SQLite access or Dapper | Not selected as the primary persistence approach; EF Core is approved for mappings and migrations. |
| NUnit or MSTest | Not selected; xUnit is the approved test framework. |
| AnyCPU | Not selected; Windows x64 is the release and validation target. |
| Framework-dependent Portable package | Not selected; Portable delivery is explicitly self-contained. |

## Consequences

### Positive

- The repository can use one supported language/runtime and one desktop UI technology.
- Composition and test seams are established before feature implementation.
- Release qualification can target a deterministic Windows architecture.
- Installer and Portable outputs can share the same solution and application contracts.

### Negative and operational impact

- WPF ties the presentation layer to Windows.
- Self-contained publishing produces larger artifacts.
- SQLite native assets and EF migrations require x64 packaging and recovery testing.
- The development and CI environments must install a compatible .NET 10 SDK.
- .NET LTS servicing updates must be applied and re-qualified throughout the support window.

## Phase 1 boundary

This ADR authorizes the build foundation only. It does not authorize Phase 2 schemas/contracts, Theme runtime behavior, EF Core persistence implementation, installer implementation, or Portable publishing profiles.
