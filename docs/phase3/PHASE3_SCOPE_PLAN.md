# Phase 3 Scope Plan

**Artifact type:** Implementation planning artifact
**Planning status:** COMPLETE
**Implementation status:** NOT STARTED
**Last updated:** 2026-09-07

## Authority

This plan is subordinate to the approved source artifacts and does not replace Product, UX, System, or Theme Architecture.

Authority used, in descending order:

1. Approved Questionnaire and Product Constitution content in the approved governance source bundle.
2. Explicit Phase 2 sequencing decision in `docs/adr/ADR-0002-phase2-schemas-public-contracts-scope.md`.
3. Frozen System Architecture v1.1.
4. Frozen Theme Architecture v1.2.
5. Phase 1 and Phase 2 approved repository baselines.

Decisive evidence:

- Phase 2 explicitly completed Frozen Theme Architecture `33.1 steps 1–3 and sealed the required schemas and public interfaces.
- Frozen Theme Architecture `33.1 lists step 4 as `Implement ThemeManifestValidator`, followed by integrity, compatibility, and capability implementations.
- Frozen Theme Architecture ``3.1, 4.2, and 5.2 assign Theme runtime implementation to `Tcc.Themes` and restrict it to `Tcc.Presentation.Contracts`.
- Frozen Theme Architecture `9.1 defines the manifest shape and semantic rejection rules.
- Frozen Theme Architecture `15.1 requires manifest validation before integrity, signature, compatibility, capability, accessibility, and activation.
- Frozen System Architecture `44.1 is explicitly labelled `ARCHITECTURE DECISION CANDIDATE`; it is not an accepted competing implementation sequence.

## Approved Baseline

- Phase 1 commit/tag: `1ea18618266abf0c99e7560ed9f23b7fb4247989` / `phase1-approved`.
- Phase 2 commit/tag: `9ea2b6e945d3f438099dcdeb922f20c2a78a48fa` / `phase2-approved`.
- Frozen System Architecture SHA-256: `7BAF7ABEBEBFFE77F20DF964E89E7845AA54E8DE810CF6FB8C6B500881275B6F`.
- Frozen Theme Architecture SHA-256: `960CCCB69AFDBE006CA12BECB86A1E3E6C96C6C59500CE943F176162D2B33856`.
- Branch start: `phase3-theme-manifest-validator` at `phase2-approved`.
- Phase 2 contracts: APPROVED / SEALED.

## Candidate Evaluation

### Candidate A — Theme Manifest Validator

- **Why next:** Frozen Theme Architecture `33.1 step 4 immediately follows completed Phase 2 steps 1–3.
- **Required prerequisites:** Phase 2 `ThemeManifest` DTO graph, schema, validation context/result, interface, contract versions, and Theme project boundary; all exist.
- **Owning projects:** `Tcc.Themes`; tests remain in `Tcc.Architecture.Tests`.
- **Contracts consumed:** `IThemeManifestValidator`, `ThemeManifest`, `ThemeManifestValidationContext`, `ThemeManifestValidationResult`, related manifest DTOs, and `ContractVersions`.
- **Runtime responsibilities:** Deterministic, fail-closed validation of a materialized manifest contract only.
- **Dependencies:** Existing `Tcc.Themes -> Tcc.Presentation.Contracts`.
- **Downstream unlocks:** Integrity verification, compatibility resolution, capability gating, safe package staging, and later Theme Library metadata.
- **Scope size:** Smallest coherent production runtime slice.
- **Risks:** Accidentally absorbing integrity, compatibility, capability, accessibility certification, package parsing/extraction, or lifecycle orchestration.
- **Disposition:** SELECTED.

### Candidate B — Theme Integrity Verifier

- **Why next:** Frozen Theme Architecture `33.1 step 5.
- **Required prerequisites:** A manifest must already have passed the step 4 validator; this prerequisite is not implemented.
- **Owning projects:** `Tcc.Themes`.
- **Contracts consumed:** `IThemeIntegrityVerifier` and integrity contracts.
- **Runtime responsibilities:** Hash/signature verification.
- **Dependencies:** Existing Theme contract edge, plus unresolved signature-policy implementation details.
- **Downstream unlocks:** Trusted package progression.
- **Scope size:** Larger security slice.
- **Risks:** Skips the required trust-state order and touches open signature-policy decisions.
- **Disposition:** DEFERRED; depends on Candidate A.

### Candidate C — Domain Primitives and Metadata

- **Why next:** Frozen System Architecture `44.1 lists it first in a global recommended build order.
- **Required prerequisites:** Approved concrete Phase scope, new project boundaries, dependency allowlist expansion, and System contract planning.
- **Owning projects:** Proposed future Domain/infrastructure projects not present in the approved Phase 1 repository graph.
- **Contracts consumed:** System contracts not materialized by sealed Phase 2.
- **Runtime responsibilities:** Broad core-domain foundation.
- **Dependencies:** Would require new projects and ProjectReferences.
- **Downstream unlocks:** Broad non-Theme application layers.
- **Scope size:** Cross-module architecture slice.
- **Risks:** The source order is an architecture-decision candidate; selecting it now would require architecture authorization and would not continue the explicit Phase 2 Theme sequence.
- **Disposition:** REJECTED for Phase 3.

## Official Phase Name

**Phase 3 — Theme Manifest Validator**

## Objective

Implement a deterministic, fail-closed `IThemeManifestValidator` in `Tcc.Themes` that validates the sealed Phase 2 `ThemeManifest` contract without loading, installing, activating, persisting, or rendering a Theme Package.

## In Scope

- Add the production `IThemeManifestValidator` implementation under `Tcc.Themes.Manifests`.
- Validate `schema_version` against `ThemeManifestValidationContext.SupportedSchemaVersions`.
- Validate `theme_api_version` against the sealed Theme API identifier required by Phase 2.
- Validate package identity, required text, channel, and semantic-version-shaped package version.
- Validate manifest collections for null/empty elements, duplicate identities, and exactly one default variant.
- Validate variant identifiers, types, token references, accessibility override references, and fallback references within the declared variant set.
- Validate capabilities against `ThemeManifestValidationContext.KnownCapabilities`; unknown or forbidden values fail closed.
- Validate presentation-only feature flags and reject any declaration capable of affecting risk, permissions, audit, recovery, connector scope, GPT optionality, authoritative data, Home Safety Core, critical-alert priority, confirmation semantics, or accessibility.
- Validate degraded-mode declarations preserve required safety/accessibility semantics and use the safe presentation fallback.
- Validate accessibility fields as declarations only; never convert a claim into platform validation.
- Validate asset, audio, motion, personalization, integrity, and rollback declarations at the manifest-contract level.
- Enforce stable-channel declaration rules available from the sealed context and contract.
- Return deterministic `Errors` and `Warnings` ordering for identical input and context.
- Add focused unit, integration, negative, determinism, smoke, and architecture-boundary coverage in the existing `Tcc.Architecture.Tests` project.
- Update Phase 3 status and implementation documentation only as required by the implementation gate.

## Explicit Non-Goals

- Raw JSON parsing, JSON Schema engine implementation, archive opening, package discovery, or filesystem reads.
- Archive extraction, quarantine, path canonicalization, zip-slip defense, package-size enforcement, promotion, installation, update, uninstall, disable, or registration.
- Integrity hashing, signature verification, compatibility resolution, capability-gate implementation, or trust-state advancement beyond manifest validation.
- Theme Library metadata store or any persistence model, database, migration, cache, or Theme state.
- Default Safe Theme, token resolution, asset loading/cache, preview, live switching, rollback execution, personalization, motion, audio, accessibility evaluator, DPI, or multi-monitor behavior.
- `ThemeApplicationService`, command pipeline, DI composition, WPF UI, Theme management views, or Gu Qinghan package/assets.
- Domain, Risk, Permission, Audit, Recovery, Security, Plugin, Connector, AI, Sync, or authoritative trading-data behavior.
- New project, `ProjectReference`, package dependency, public API, schema, enum, DTO, or sealed Phase 2 semantic change.
- Commit, tag, push, merge, release, or deployment.

## Ownership

| Project | Namespace | Responsibility |
|---|---|---|
| `Tcc.Themes` | `Tcc.Themes.Manifests` | Production manifest-validation implementation and implementation-only validation helpers. |
| `Tcc.Presentation.Contracts` | `Tcc.Presentation.Contracts.Theme` | Existing sealed interfaces, DTOs, value identifiers, and contract constants; consume only, do not modify. |
| `Tcc.Architecture.Tests` | `Tcc.Architecture.Tests` | Focused runtime, negative, determinism, integration, smoke, and architecture-boundary tests. |

## Contracts Consumed

Phase 2 public contracts:

- `IThemeManifestValidator`
- `ThemeManifestValidationContext`
- `ThemeManifestValidationResult`
- `ThemeManifest`
- `ThemePackageIdentity`
- `ThemeCompatibilityDeclaration`
- `ThemeVariant`
- `ThemeFeatureFlags`
- `ThemePresentationFeatureFlag`
- `ThemeDegradedModeDeclaration`
- `ThemeAccessibilityDeclaration`
- `ThemeAssetDeclaration`
- `ThemeAudioDeclaration`
- `ThemeMotionDeclaration`
- `ThemePersonalizationDeclaration`
- `ThemeIntegrityDeclaration`
- `ThemeRollbackDeclaration`
- `ContractVersions`

Phase 2 declarative contracts:

- `contracts/theme/schemas/ThemeManifest.schema.json`
- `contracts/theme/theme-manifest.schema.json` as the generated projection
- Accessibility, motion, and audio invariant contracts only as manifest-validation constraints; no runtime engine implementation.

Contract stability classification: **A — Complete**. No public contract or schema semantic change is required. Implementation-only validation rules, normalization-free comparisons, deterministic diagnostics, and private helper structure remain internal to `Tcc.Themes`.

## Runtime Responsibilities

- Accept an already materialized `ThemeManifest` and a sealed `ThemeManifestValidationContext`.
- Evaluate the manifest contract without I/O or mutation.
- Return `IsValid = false` for unsupported schema versions, unknown/forbidden capabilities, or violated manifest safety invariants.
- Keep diagnostics deterministic and suitable for later audit/diagnostics correlation without emitting audit events in this phase.
- Treat Q93 fields as publisher declarations; platform accessibility validation remains a later distinct runtime.

## Dependency Graph

```text
Tcc.Architecture.Tests
  -> Tcc.Themes
  -> Tcc.Presentation.Contracts

Tcc.Themes
  -> Tcc.Presentation.Contracts
```

Both direct edges already exist and are allowed by the Phase 1 dependency boundary.

- New `ProjectReference` required: NO.
- New package required: NO.
- Architecture-authorized dependency change candidate: NONE.
- Potentially illegal dependency: NONE.

## Data Ownership

NONE. This phase performs pure validation and owns no persisted state, cache, authoritative data, Theme Library metadata, or lifecycle record.

## Safety Boundaries

- **Risk:** No risk rules, evaluation, mutation, or presentation-semantic changes.
- **Permission:** No permission rules, authorization bypass, or lifecycle command authorization.
- **Audit:** No audit mutation or claims that material actions were audited; validator diagnostics are not audit events.
- **Recovery:** No Safe Mode or Recovery Core ownership; no rollback execution.
- **Theme isolation:** `Tcc.Themes` consumes presentation contracts only and never depends on Domain, Application, Persistence, Security, Recovery, Plugin, Connector, AI, Sync, Risk, Permission, or Audit implementations.
- **Plugin sandbox:** No executable Theme Package code and no plugin capability.
- **Connector read-only:** No connector API or execution/write surface.
- **GPT optionality:** No GPT/API requirement or AI authority.
- **Q93:** Manifest accessibility claims remain unvalidated until a later platform-run accessibility validator passes.
- **Home Safety Core:** Feature flags and degraded mode cannot hide or weaken it.

## Failure Semantics

- Invalid, unsupported, unknown, or safety-ambiguous manifest declarations fail closed.
- Invalid input returns `ThemeManifestValidationResult` with deterministic errors; it does not install, persist, activate, or partially accept a theme.
- No silent normalization changes identifiers, versions, paths, capabilities, or safety declarations.
- Warnings may describe non-blocking declarations only and cannot downgrade a required error.
- The active theme is unaffected because this phase has no lifecycle or state mutation.
- Recovery responsibility remains with later Theme lifecycle/recovery integration; this validator only reports failure.

## Test Strategy

- **Unit:** Each field group and cross-field invariant has positive and boundary coverage.
- **Integration:** The production validator is exercised against actual Phase 2 DTOs, contract constants, and representative schema-valid manifest objects.
- **Architecture:** Existing repository allowlist passes; no forbidden assembly dependency or new project appears.
- **Negative:** Unsupported schema/API versions, invalid identity/version/channel, duplicate/default/fallback errors, unknown capabilities, forbidden semantic reach, weakened degraded-mode safety, false accessibility elevation, invalid asset/audio/motion/personalization/integrity/rollback declarations.
- **Determinism:** Identical input/context produces byte-for-byte equivalent ordered errors and warnings across repeated runs.
- **Smoke:** A minimum valid manifest passes, and a minimum invalid manifest fails closed through `IThemeManifestValidator`.
- **Regression:** All Phase 1 and Phase 2 tests remain green, including schema, serialization, public API, and dependency guards.

## Acceptance Gate

Phase 3 may enter Independent Validation only when all of the following are true:

1. The implementation covers every item in **In Scope** and none in **Explicit Non-Goals**.
2. No sealed Phase 2 public contract or schema changed.
3. No project, `ProjectReference`, package, lock file, WPF UI, or production assembly outside `Tcc.Themes` changed.
4. Focused unit/integration/negative/determinism/smoke tests pass.
5. Locked restore passes.
6. Release x64 build passes with 0 warnings and 0 errors.
7. The complete test suite passes with 0 failed and 0 skipped.
8. Architecture dependency and forbidden-surface guards pass.
9. Frozen System and Theme hashes remain exact.
10. Scope/leakage, secret/local-path, temporary-artifact, staged-file, and `git diff --check` gates pass.
11. `docs/CODEX_PROJECT_STATUS.md` truthfully records the implementation and validation state.
12. No blocker is hidden and no commit, tag, push, merge, release, or deploy has occurred.

## Phase Leakage Guard

If a production change does not belong to deterministic, side-effect-free `ThemeManifest` validation in `Tcc.Themes.Manifests`, it is Phase 4+ leakage.

The following are explicitly prohibited from Phase 3 production implementation:

- `ThemeIntegrityVerifier`, `ThemeCompatibilityResolver`, and `ThemeCapabilityGate`.
- Theme package parsing/loading, archive safety, quarantine, install/update/uninstall/disable, store registration, and lifecycle orchestration.
- Theme Library metadata persistence and all database/schema/migration work.
- Default Safe Theme, token/layout/component resolution, assets/cache, preview, live switching, rollback execution, personalization, motion, audio, accessibility evaluation, diagnostics runtime, Windows adapters, and Safe Mode hooks.
- Theme UI, DesktopHost composition changes, Gu Qinghan assets/package, plugin sandbox, connectors, AI/GPT, notifications, release tooling, migration, and any Domain/Application/Trading feature.

### AI Trading Intelligence Leakage Guard

The AI Trading Intelligence Baseline v1 is a separate future Architecture Amendment Track. It does not modify or expand Phase 3 production scope.

Theme Manifest Validator production code, tests, contracts, schemas, and dependencies must not introduce or depend on:

- Trading AI or AI Recommendation.
- Market Observation or Market Data.
- Trading Risk Policy.
- Position Intelligence.
- Shadow Trading or Learning.
- Prepared Orders.
- Trade Execution.

The Theme Runtime Track and Trading Intelligence Track remain separate. Future Trading/AI contracts must be additive and must not enter the Theme contract family or change the sealed `IThemeManifestValidator` contract.

## Known Risks

- The public validator contract accepts an already materialized DTO, so raw JSON parsing and `additionalProperties` enforcement stay outside this phase; tests must not imply that the validator is an archive or JSON parser.
- Validator rules must not duplicate later compatibility, integrity, capability-gate, or accessibility-certification responsibilities.
- String diagnostics are public result data; deterministic content and ordering are required even though implementation-only helpers remain private.
- Frozen Theme examples contain historical version display text; sealed Phase 2 `ContractVersions` and schemas remain the machine-readable authority used by implementation.

## Open Decisions

NONE for this phase. Theme archive format, signature authority, extra hash algorithms, cache thresholds, visual tooling, store protocol, quarantine design, installer technology, and other recorded OTDs remain deferred because the validator does not require them.

## Exact Next Action

Return to GPT Supervisor for 05A validation. Do not start Phase 3 implementation without separate authorization.
