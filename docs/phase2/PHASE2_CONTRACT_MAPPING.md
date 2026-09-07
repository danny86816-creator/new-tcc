# Phase 2 Contract Mapping

**Status:** Implementation mapping
**Authority:** Explicit Phase 2 scope decision plus Frozen Theme Architecture v1.2
**Contract owner:** `Tcc.Presentation.Contracts`

## §33.2 schema files

All paths are under `contracts/theme/schemas/`. All schema files are owned as presentation contract artifacts by `Tcc.Presentation.Contracts`; JSON artifacts do not have a C# namespace. The `ThemeCopyResources` and `ThemeCriticalCopyRules` repetitions in §33.2 are one file each.

| Required Item | Frozen Source Reference | Target Path | Namespace | Owning Assembly | Test | Acceptance Condition |
|---|---|---|---|---|---|---|
| `ThemeManifest.schema.json` | Theme §§7.7, 9.1 | `contracts/theme/schemas/ThemeManifest.schema.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests`, `PhaseTwoPublicApiGuardTests` | Parses; minimum valid passes; missing required/forbidden capability fails |
| `ThemeIntegrity.schema.json` | Theme §§9.2, 15.2 | `contracts/theme/schemas/ThemeIntegrity.schema.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests` | Hash, file inventory, identity, timestamp required |
| `ThemeCompatibility.schema.json` | Theme §§9.3, 10.4–10.5 | `contracts/theme/schemas/ThemeCompatibility.schema.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests`, `PhaseTwoUxContractTests` | Core/API/UX/Windows/Q93 fields required; unsupported versions fail closed |
| `ThemeRollback.schema.json` | Theme §14.6 | `contracts/theme/schemas/ThemeRollback.schema.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests` | Verified target metadata and theme-only state policy required |
| `ThemeAssets.schema.json` | Theme §§9.4, 18 | `contracts/theme/schemas/ThemeAssets.schema.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests` | Inventory, fallback, integrity, tiers, and all budget fields required |
| `ThemeTokens.schema.json` | Theme §29.3 | `contracts/theme/schemas/ThemeTokens.schema.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests` | Token values and semantic bindings parse and validate |
| `ThemeLayoutAdapter.schema.json` | Theme §§6.4–6.5, 29.4 | `contracts/theme/schemas/ThemeLayoutAdapter.schema.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests` | Stable surface/zone IDs and keyboard/focus contract required |
| `ThemeComponentAdapter.schema.json` | Theme §§7.3, 29.4 | `contracts/theme/schemas/ThemeComponentAdapter.schema.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests` | Surface scope, preserved semantics, keyboard/focus required |
| `ThemeCopyResources.schema.json` | Theme §23.5.1 | `contracts/theme/schemas/ThemeCopyResources.schema.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests` | Locale, noncritical resources, approved critical fallbacks required |
| `ThemeCriticalCopyRules.schema.json` | Theme §§23.5–23.5.1 | `contracts/theme/schemas/ThemeCriticalCopyRules.schema.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests` | Core-owned keys and fail-closed plain-language rules required |
| `ThemeFocusStyles.schema.json` | Theme §§22.3, 29.4 | `contracts/theme/schemas/ThemeFocusStyles.schema.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests` | 3:1 minimum and focus/order/trap/restoration invariants required |
| `ThemeStatePresentation.schema.json` | Theme §§6.6, 23 | `contracts/theme/schemas/ThemeStatePresentation.schema.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests` | Stable state ID, meaning, redundancy, and limited freedom required |
| `ThemeMotionProfile.schema.json` | Theme §§9.6, 19, 21 | `contracts/theme/schemas/ThemeMotionProfile.schema.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests`, `PhaseTwoPublicApiGuardTests` | Interrupt/skip/reduced fallback/supplementary/no-loss rules required |
| `ThemeSoundPack.schema.json` | Theme §§9.7, 20 | `contracts/theme/schemas/ThemeSoundPack.schema.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests`, `PhaseTwoPublicApiGuardTests` | Independent controls, disabled-state respect, ambient/BGM, no-loss metadata required |
| `ThemePersonalizationSafeRanges.schema.json` | Theme §§9.5, 17 | `contracts/theme/schemas/ThemePersonalizationSafeRanges.schema.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests` | Safe controls and forbidden semantic effects required |
| `ThemeRuntimeState.schema.json` | Theme §§12.1–12.4, 20.3 | `contracts/theme/schemas/ThemeRuntimeState.schema.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests` | Theme-only scope plus independent audio/motion/automation fields required |
| `UxSurfaceContract.schema.json` | Theme §§6.1–6.4, 30 | `contracts/theme/schemas/UxSurfaceContract.schema.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests`, `PhaseTwoUxContractTests` | Every approved page contract validates with provenance and Q93 obligations |
| `FunctionalZoneContract.schema.json` | Theme §§6.5, 30; UX §10.4 | `contracts/theme/schemas/FunctionalZoneContract.schema.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests`, `PhaseTwoUxContractTests` | All 17 approved zones validate; Safety Core children fixed |
| `GlobalStatePresentation.schema.json` | Theme §6.6; UX §36.1 | `contracts/theme/schemas/GlobalStatePresentation.schema.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests`, `PhaseTwoUxContractTests` | All 19 approved states validate with redundant cues |
| `ThemeDiagnosticsEvent.schema.json` | Theme §28.2 | `contracts/theme/schemas/ThemeDiagnosticsEvent.schema.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests` | Theme identifiers and privacy-false defaults required; trading payload absent |
| `CopyFallbackResult.schema.json` | Theme §23.5.1 | `contracts/theme/schemas/CopyFallbackResult.schema.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests` | Source, locale, critical/fallback decision, valid result required |

## §33.3 public interfaces

All interfaces target `src/Tcc.Presentation.Contracts/Theme/ThemeInterfaces.cs`, namespace `Tcc.Presentation.Contracts.Theme`, assembly `Tcc.Presentation.Contracts`, and are tested by `PhaseTwoContractCompletenessTests` plus `PhaseTwoPublicApiGuardTests`.

| Required Item | Frozen Source Reference | Target Path | Namespace | Owning Assembly | Test | Acceptance Condition |
|---|---|---|---|---|---|---|
| `IThemePackage` | Theme §§7.3, 29 | `src/Tcc.Presentation.Contracts/Theme/ThemeInterfaces.cs` | `Tcc.Presentation.Contracts.Theme` | `Tcc.Presentation.Contracts` | Completeness/API guard | Public declarative facade; no package executable implementation |
| `IThemeRuntime` | Theme §7.5 | same | same | same | Completeness/API guard | Lifecycle contract only; no implementation |
| `IThemeManifestValidator` | Theme §§9.1, 15.1 | same | same | same | Completeness/API guard | Validation result contract only |
| `IThemeIntegrityVerifier` | Theme §§9.2, 15.2 | same | same | same | Completeness/API guard | Integrity/signature result contract only |
| `IThemeCompatibilityResolver` | Theme §10 | same | same | same | Completeness/API guard | Deterministic compatibility result contract only |
| `IThemeCapabilityGate` | Theme §§7.7, 15.5 | same | same | same | Completeness/API guard | Allow/block/unknown contract; unknown fails closed |
| `IThemeAssetLoader` | Theme §18.3 | same | same | same | Completeness/API guard | Tier/load/preload/unload contract only |
| `IThemeAssetCache` | Theme §§18.4–18.7 | same | same | same | Completeness/API guard | Cache lookup/store/clear/validate contract only |
| `IThemePreviewSandbox` | Theme §16 | same | same | same | Completeness/API guard | Permission-filtered read-only snapshot and isolated preview state |
| `ILiveThemeSwitchCoordinator` | Theme §13 | same | same | same | Completeness/API guard | Switch/interrupt contract with Safety Core continuity |
| `IThemePersonalizationEngine` | Theme §17 | same | same | same | Completeness/API guard | Validate/clamp/reset presentation-only values |
| `IThemeMotionManager` | Theme §19.4 | same | same | same | Completeness/API guard | Resolve/reduce/interrupt/skip presentation motion |
| `IThemeAudioRouter` | Theme §§20, 29.5 | same | same | same | Completeness/API guard | Independent master/BGM/ambient/UI/alert/preview contracts |
| `IThemeAccessibilityValidator` | Theme §§21–22 | same | same | same | Completeness/API guard | Platform validation result contract; claims are not validation |
| `IThemeRollbackManager` | Theme §14.6 | same | same | same | Completeness/API guard | Verified rollback options and result contract only |
| `IThemeRecoveryHooks` | Theme §29.6 | same | same | same | Completeness/API guard | Theme-specific hooks only; Recovery Core ownership unchanged |
| `IThemeDiagnosticsEmitter` | Theme §28 | same | same | same | Completeness/API guard | Emits redacted Theme diagnostics; no mutation authority |

## §33.1 stable UX contract artifacts

| Required Item | Frozen Source Reference | Target Path | Namespace | Owning Assembly | Test | Acceptance Condition |
|---|---|---|---|---|---|---|
| Stable UX contract registry | Theme §§6, 30; UX §§3.1, 7.1, 8.2, 9.1, 10.4, 36.1, 37.1 | `contracts/theme/ux-contract.v1.1.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoUxContractTests` | Exact sets: 15 domains, 9 workspace IDs/patterns, 41 modules, 71 pages, 17 zones, 19 states, 32 flows; no duplicate/invented ID |
| Per-page contracts | Theme §§6.4, 30.2 | `contracts/theme/page-contracts/*.json` | N/A | `Tcc.Presentation.Contracts` | `PhaseTwoSchemaTests` | 71 files; each validates and includes provenance/accessibility/safety/test coverage |
| Per-module contracts | Theme §6.7; UX §8.2 | `contracts/theme/module-contracts/*.json` | N/A | `Tcc.Presentation.Contracts` | Completeness/UX tests | 41 files with exact ID, meaning, provenance, and semantic guards |
| Per-zone contracts | Theme §§6.5, 6.7; UX §10.4 | `contracts/theme/zone-contracts/*.json` | N/A | `Tcc.Presentation.Contracts` | Schema/UX tests | 17 files; Safety Core contains the four fixed children |
| Per-state contracts | Theme §§6.6, 6.7; UX §36.1 | `contracts/theme/state-contracts/*.json` | N/A | `Tcc.Presentation.Contracts` | Schema/UX tests | 19 files; text/structure/programmatic/screen-reader redundancy required |
| Accessibility contract | Theme §§21–22 | `contracts/theme/accessibility-contract.v1.json` | N/A | `Tcc.Presentation.Contracts` | API guard/UX tests | Q93 fields and normative contrast/scale/zoom requirements materialized |
| Motion contract | Theme §19 | `contracts/theme/motion-contract.v1.json` | N/A | `Tcc.Presentation.Contracts` | API guard | Interruptible/skippable/reduced/supplementary/no-loss invariants materialized |
| Audio contract | Theme §20 | `contracts/theme/audio-contract.v1.json` | N/A | `Tcc.Presentation.Contracts` | API guard | Independent controls, one-click disable, redundancy, and no-loss invariants materialized |
| Deterministic generator | Theme §6.7 | `tools/phase2/Generate-ThemeContracts.ps1` | N/A | Build tooling | Generator completeness guard | Regeneration uses repository-relative paths, rejects duplicate IDs, and asserts all schemas exist |
