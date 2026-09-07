# Codex Project Status

**Current approved phase:** Phase 2 — Schemas & Public Contracts
**Current execution status:** Phase 2 implementation complete; independent re-validation passed; ready for baseline commit preparation
**Last updated:** 2026-09-07

**Commit allowed:** YES
**Phase 3 allowed:** NO
**Git branch:** `phase2-foundation`
**Baseline HEAD:** `1ea18618266abf0c99e7560ed9f23b7fb4247989`

## Baseline

- Theme Architecture governance baseline: `v1.2 — APPROVED`.
- Approved SHA-256: `960CCCB69AFDBE006CA12BECB86A1E3E6C96C6C59500CE943F176162D2B33856`.
- Baseline interpretation: `docs/governance/TCC_THEME_ARCHITECTURE_BASELINE_APPROVAL.md`.
- Technology decision: `docs/adr/ADR-0001-platform-technology-stack.md`.
- Theme feature ownership decision: `docs/adr/ADR-0002-theme-feature-owner.md`.
- Phase 2 scope decision: `docs/adr/ADR-0002-phase2-schemas-public-contracts-scope.md`.
- Frozen System Architecture SHA-256: `7BAF7ABEBEBFFE77F20DF964E89E7845AA54E8DE810CF6FB8C6B500881275B6F`.

## Phase 1 repository foundation

```text
Tcc.slnx
src/
  Tcc.Presentation.Contracts/
  Tcc.Themes/
  Tcc.Features.Themes/
  Tcc.Windows/
  Tcc.DesktopHost/
tests/
  Tcc.Architecture.Tests/
docs/
  adr/
  governance/
```

## Implemented in Phase 1

- .NET 10 Windows x64 build policy.
- WPF desktop host shell using Generic Host and dependency injection.
- Minimal MVVM host view model.
- Approved assembly/package boundaries.
- xUnit test runner and initial dependency-boundary smoke tests.
- Governance baseline and decision records.

## Phase 2 exact scope

- Theme Architecture §33.1 steps 1–3.
- All 21 unique schema/contract files required by §33.2.
- All 17 public interfaces required by §33.3.
- Stable UX contract registry and deterministic page/module/zone/state projections.
- Contract, schema, negative, serialization, API-surface, and architecture tests in the existing test project.

## Phase 2 implemented contracts

- `Tcc.Presentation.Contracts.Theme` version identifiers, value types, enums, DTOs, result contracts, and 17 required interfaces.
- 21 strict JSON Schema files covering manifest, integrity, compatibility, rollback, assets, tokens, layout/components, copy, focus, state, motion, audio, personalization, runtime state, UX/zone/state, diagnostics, and copy fallback.
- Stable UX registry: 15 domains, 9 workspace IDs/patterns, 41 modules, 71 pages, 17 zones, 19 states, and 32 flows.
- 71 page contracts plus module/zone/state projections with approved provenance.
- Q93 accessibility, motion, audio, Home Safety Core, permission/risk/confirmation, GPT optionality, and read-only connector constraints.

## Phase 2 non-goals

- Production Theme validation, integrity, compatibility, capability, or sandbox logic.
- Theme persistence or EF Core model/migrations.
- Default safe theme, switching, preview, library, personalization, assets, cache, motion, or audio.
- Recovery/Safe Mode integration.
- Installer or Portable publishing implementation.
- Theme UI, Gu Qinghan package/assets, or Phase 3+ production behavior.

## UX contract provenance

- IDs and meanings derive from `TCC Information & UX Architecture v1.1 — APPROVED` stable registries (§§3.1, 7.1, 8.2, 9.1, 10.4, 36.1, 37.1).
- Per-surface Theme constraints derive from `TCC Theme Architecture v1.2 — APPROVED` §§6 and 30.2.
- No new UX ID is introduced. Duplicate IDs are rejected by generator and negative test.

## Current Phase 2 validation state

- Implementation: **COMPLETE**.
- Initial independent validation verdict: **FAIL** (`P2-VAL-001` HIGH, `P2-VAL-002` HIGH, `P2-VAL-003` HIGH, `P2-VAL-004` MEDIUM / commit blocker).
- Surgical remediation: **COMPLETE** for all four identified defects.
- Current independent validation status: **PASS**.
- Independent re-validation verdict: **PASS** — CRITICAL 0, HIGH 0, MEDIUM 0, LOW 0.
- Previous defects: `P2-VAL-001` **RESOLVED**; `P2-VAL-002` **RESOLVED**; `P2-VAL-003` **RESOLVED**; `P2-VAL-004` **RESOLVED**.
- Phase 2 completeness: **100%** — 21/21 schemas and 17/17 public interfaces.
- Open defects: **NONE**.
- Commit allowed: **YES**.
- Phase 3 allowed: **NO** until the Phase 2 baseline commit, `phase2-approved` tag, and Phase 3 branch are created.
- Actual SDK: system-installed `.NET SDK 10.0.400` at `C:\Program Files\dotnet\dotnet.exe`.
- `global.json`: present; minimum `10.0.100`, `rollForward: latestFeature`, resolves to `10.0.400`.

## Phase 2 surgical remediation

- `P2-VAL-001`: aligned the 18 material DTO/schema counterparts, including nested properties, required/optional nullability, Theme Manifest feature/degraded/audio fields, canonical `token_file`, runtime personalization/audio shape, string value-object JSON, `CopyFallbackResult.schema_version`, motion document shape, and rollback migration nullability.
- `P2-VAL-002`: corrected generator-owned exact Frozen state mappings for `UX-HOME-001` and `UX-REVIEW-006`; all 71 generated pages now have exact-set regression coverage.
- `P2-VAL-003`: Theme API is fixed to `1.0.0`; page, zone, global-state, and applicable surface-scope identifiers are generated enums; workspace IDs use the fixed approved set plus only the explicit `WS-CUSTOM-*` family.
- `P2-VAL-004`: removed the hard-coded compatibility helper, mutation-tests actual schemas/artifacts, checks every root required property individually, rejects unsupported schema keywords explicitly, and executes the real mutated generator for duplicate-ID rejection.

## Phase 2 validation record

| Check | Result |
|---|---|
| Release x64 clean | Passed — 0 warnings, 0 errors |
| Normal restore | Passed — all six projects restored |
| Locked restore | Passed — all six projects restored with `--locked-mode` |
| Release x64 build | Passed — 0 warnings, 0 errors |
| Complete test suite | Passed — 94/94, 0 failed, 0 skipped |
| Dependency boundary tests | Passed — 5/5 |
| Contract completeness tests | Passed — 4/4 |
| Schema and negative tests | Passed — 51/51 |
| UX registry tests | Passed — 4/4 |
| Serialization and DTO/schema integration tests | Passed — 24/24 |
| Public API guard tests | Passed — 4/4 |
| JSON parse and deterministic generation | Passed — 174 JSON files; 0 hash changes after regeneration |
| Frozen System Architecture hash | Passed — `7BAF7ABEBEBFFE77F20DF964E89E7845AA54E8DE810CF6FB8C6B500881275B6F` |
| Frozen Theme Architecture hash | Passed — `960CCCB69AFDBE006CA12BECB86A1E3E6C96C6C59500CE943F176162D2B33856` |
| Forbidden API/runtime leakage scan | Passed — no matches |
| Secret/local-path/temporary-SDK scan | Passed — no matches |
| Project/lock-file changes | Passed — none |

## Current Phase 2 change areas

- `contracts/theme/`
- `docs/adr/ADR-0002-phase2-schemas-public-contracts-scope.md`
- `docs/phase2/PHASE2_CONTRACT_MAPPING.md`
- `README.md`
- `docs/CODEX_DECISIONS.md`
- `docs/CODEX_PROJECT_STATUS.md`
- `src/Tcc.Presentation.Contracts/Theme/`
- `tests/Tcc.Architecture.Tests/PhaseTwo*.cs`
- `tests/Tcc.Architecture.Tests/JsonSchemaSubsetValidator.cs`
- `tests/Tcc.Architecture.Tests/RepositoryPaths.cs`
- `tools/phase2/Generate-ThemeContracts.ps1`

## Tests added

- `PhaseTwoContractCompletenessTests`: exact §33.2 schema and §33.3 interface inventory, ownership, and no-production-implementation guards.
- `PhaseTwoSchemaTests`: parse/valid-minimum checks, every required-property mutation, generated artifacts, capability/Q93 constraints, actual-schema version/invented-ID mutations, explicit wildcard handling, and fail-fast unsupported-keyword coverage.
- `PhaseTwoUxContractTests`: exact approved ID sets, all-page Frozen exact-state equality, and actual mutated-generator duplicate-ID rejection with fixture cleanup.
- `PhaseTwoSerializationTests`: deterministic serialization plus mechanical property/required/nullability and complete/minimum wire-shape audit across 18 DTO/schema counterparts.
- `PhaseTwoPublicApiGuardTests`: forbidden mutation API, presentation-only capability, read-only connector, motion, and audio guards.
- `JsonSchemaSubsetValidator` and `RepositoryPaths`: test-only schema validation and repository path support.

## Validation commands

- `dotnet clean Tcc.slnx --configuration Release -p:Platform=x64 -m:1`
- `dotnet restore Tcc.slnx --force --no-cache -p:RestoreDisableParallel=true -p:Platform=x64 -m:1`
- `dotnet restore Tcc.slnx --locked-mode --force --no-cache -p:RestoreDisableParallel=true -p:Platform=x64 -m:1`
- `dotnet build Tcc.slnx --configuration Release --no-restore -p:Platform=x64 -m:1`
- `dotnet test Tcc.slnx --configuration Release --no-build -p:Platform=x64 -m:1`
- Filtered `dotnet test` runs for dependency boundaries, completeness, schemas, UX, serialization, public API guards, and negative cases.
- `tools/phase2/Generate-ThemeContracts.ps1` plus before/after SHA-256 comparison.
- Frozen SHA-256, forbidden API, runtime/scope leakage, secret, local-path, temporary-SDK, lock-file, project-graph, `git diff --check`, `git diff`, and full `git status` checks.

## Current risks and blockers

- Open Phase 2 defects or validation blockers: none.
- Unrestricted solution-level MSBuild previously caused abnormal recursive process growth on this host; all remediation gates used `-m:1`. This is recorded as host/invocation behavior, not treated as a Phase 2 product defect.
- The Phase 2 baseline commit and `phase2-approved` tag have not yet been created.
- Phase 3 remains prohibited until the baseline commit, approval tag, and Phase 3 branch prerequisites are complete.

## Exact next action

Prepare the Phase 2 baseline commit. Do not create the commit, `phase2-approved` tag, or Phase 3 branch without explicit authorization.
